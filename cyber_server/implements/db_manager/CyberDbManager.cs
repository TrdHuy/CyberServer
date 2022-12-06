using cyber_server.@base;
using cyber_server.definition;
using cyber_server.implements.log_manager;
using cyber_server.utils;
using cyber_server.views.windows.others;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace cyber_server.implements.db_manager
{
    internal class CyberDbManager : IServerModule
    {

        private string _connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename="
            + CyberServerDefinition.DB_FILE_PATH + @";Integrated Security=True;Connect Timeout=30";
        private SemaphoreSlim _semaphoreForAppDbContext;

        /// <summary>
        /// Do not use this directly, use it via RequestDbContextAsync
        /// for synchronization
        /// </summary>
        private CyberDragonDbContext _appDbContext;
        public static CyberDbManager Current
        {
            get { return ServerModuleManager.DBM_Instance; }
        }

        private CyberDbManager()
        {
            _appDbContext = new CyberDragonDbContext();

            // Preload db when first boot app
            foreach (var plugin in _appDbContext.Plugins)
            {
                break;
            }
            _semaphoreForAppDbContext = new SemaphoreSlim(1, 1);
        }

        public void Dispose()
        {
            _appDbContext.Dispose();
        }

        public void OnModuleInit()
        {
        }

        public async Task<bool> RequestDbContextAsync(Action<CyberDragonDbContext> request)
        {
            var isSucess = false;
            await _semaphoreForAppDbContext.WaitAsync();
            try
            {
                request.Invoke(_appDbContext);
                isSucess = true;
            }
            catch (Exception ex)
            {
                isSucess = false;
                MessageBox.Show(ex.Message, "Lỗi!");
                ServerLogManager.Current.E(ex.ToString());
            }
            finally
            {
                _semaphoreForAppDbContext.Release();
            }
            return isSucess;
        }

        public bool RollBack(bool force = true)
        {
            var changedEntries = _appDbContext.ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Unchanged).ToList();
            var confirm = false;
            foreach (var entry in changedEntries)
            {
                if ((entry.State == EntityState.Modified
                    || entry.State == EntityState.Added
                    || entry.State == EntityState.Deleted)
                    && !confirm
                    && !force)
                {
                    confirm = MessageBox.Show("Hoàn tác thay đổi?", "Xác nhận",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                    if (!confirm)
                        return false;
                }

                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
            return true;
        }

        public async Task<int> SaveChanges()
        {
            return await _appDbContext.SaveChangesAsync();
        }

        public async Task<List<string>> Backup(string savingLocationPath)
        {
            using (SqlConnection cnn = new SqlConnection(_connectionString))
            {
                await cnn.OpenAsync();
                List<string> tables = new List<string>();
                var optionLst = new List<string>();
                {
                    DataTable dt = cnn.GetSchema("Tables");
                    foreach (DataRow row in dt.Rows)
                    {
                        string tablename = (string)row[2];
                        tables.Add(tablename);
                    }
                    optionLst = new MultiOptionWindow(tables.ToArray(), true)
                    {
                        Title = "Chọn table mà bạn muốn backup",
                        Owner = App.Current.ServerWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    }.ShowDialog();
                    if (optionLst.Count == 0) return null;
                }

                var exportFilePathLst = new List<string>();
                foreach (var tableName in optionLst)
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM " + tableName))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.Connection = cnn;
                            sda.SelectCommand = cmd;
                            var exportFilePath = savingLocationPath + "\\" + tableName + ".csv";

                            using (StreamWriter sw = new StreamWriter(exportFilePath))
                            {
                                using (DataTable dt = new DataTable())
                                {
                                    sda.Fill(dt);

                                    //Build the CSV file data as a Comma separated string.
                                    string csv = string.Empty;

                                    foreach (DataColumn column in dt.Columns)
                                    {
                                        //Add the Header row for CSV file.
                                        csv += column.ColumnName + "(" + column.DataType.FullName + ")" + ',';
                                    }
                                    //Add new line.
                                    csv += "\r\n";
                                    sw.Write(csv);

                                    foreach (DataRow row in dt.Rows)
                                    {
                                        csv = string.Empty;
                                        foreach (DataColumn column in dt.Columns)
                                        {

                                            if (row[column.ColumnName] is byte[])
                                            {
                                                var hexString = BitConverter.ToString((byte[])row[column.ColumnName]);
                                                csv += hexString + ",";
                                            }
                                            else
                                            {
                                                //Add the Data rows.
                                                csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';
                                            }
                                        }
                                        //Add new line.
                                        csv += "\r\n";
                                        sw.Write(csv);
                                    }
                                    exportFilePathLst.Add(exportFilePath);
                                }
                            }

                        }
                    }
                }

                return exportFilePathLst;
            }
        }

        public async Task ImportCSVToDb(string[] csvFilePaths)
        {

            byte[] GetBytesFromString(string str)
            {
                String[] arr = str.Split('-');
                byte[] array = new byte[arr.Length];
                for (int i = 0; i < arr.Length; i++) array[i] = Convert.ToByte(arr[i], 16);
                return array;
            }

            /// Ineffective parsing method 
            async Task<DataTable> GetDataTabletFromCSVFile(string filePath)
            {
                await Task.Delay(5000);
                DataTable csvData = new DataTable();
                try
                {
                    using (TextFieldParser csvReader = new TextFieldParser(filePath))
                    {
                        csvReader.SetDelimiters(new string[] { "," });
                        csvReader.HasFieldsEnclosedInQuotes = true;
                        string[] colFields = csvReader.ReadFields();
                        var columnRegex = new Regex("(?<Name>[a-zA-Z]*)\\(" +
                            "(?<Type>(System.Int64)|(System.Boolean)|(System.DateTime)|(System.Int32)|(System.Byte\\[\\])|(System.String))\\)");
                        string[] colType = new string[colFields.Length];
                        int[] selectedCol = new int[colFields.Length];
                        int selectedDataCount = 0;
                        foreach (string column in colFields)
                        {
                            var match = columnRegex.Matches(column);
                            if (match.Count > 0)
                            {
                                DataColumn dataColumn = new DataColumn(match[0].Groups["Name"].Value, Type.GetType(match[0].Groups["Type"].Value));
                                dataColumn.AllowDBNull = true;
                                csvData.Columns.Add(dataColumn);
                                selectedCol[selectedDataCount] = selectedDataCount;
                                colType[selectedDataCount++] = match[0].Groups["Type"].Value;
                            }
                        }

                        while (!csvReader.EndOfData)
                        {
                            string[] fieldData = csvReader.ReadFields();
                            object[] refactorData = new object[selectedDataCount];
                            //Making empty value as null
                            for (int i = 0; i < selectedDataCount; i++)
                            {
                                switch (colType[i])
                                {
                                    case "System.Byte[]":
                                        {
                                            var bytesString = fieldData[selectedCol[i]];
                                            refactorData[selectedCol[i]] = GetBytesFromString(bytesString);
                                            break;
                                        }
                                    default:
                                        {
                                            refactorData[selectedCol[i]]
                                                = fieldData[selectedCol[i]] == "" ? null : fieldData[selectedCol[i]];
                                            break;
                                        }
                                }
                            }
                            csvData.Rows.Add(refactorData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
                return csvData;
            }
            /// Effective method
            async Task<DataTable> GetDataTabletFromCSVFile2(string filePath)
            {
                DataTable csvData = new DataTable();
                try
                {
                    using (var streamReader = File.OpenText(filePath))
                    {
                        string line;
                        var isColumnTitleRow = true;
                        var columnRegex = new Regex("(?<Name>[a-zA-Z]*)\\(" +
                                    "(?<Type>(System.Int64)|(System.Boolean)|(System.DateTime)|(System.Int32)|(System.Byte\\[\\])|(System.String))\\)");

                        string[] colType = new string[0];
                        int[] selectedCol = new int[0];
                        int selectedDataCount = 0;

                        while ((line = await streamReader.ReadLineAsync()) != null)
                        {
                            if (isColumnTitleRow)
                            {
                                var colFields = line.Split(',');
                                colType = new string[colFields.Length];
                                selectedCol = new int[colFields.Length];
                                foreach (string column in colFields)
                                {
                                    var match = columnRegex.Matches(column);
                                    if (match.Count > 0)
                                    {
                                        DataColumn dataColumn = new DataColumn(match[0].Groups["Name"].Value, Type.GetType(match[0].Groups["Type"].Value));
                                        dataColumn.AllowDBNull = true;
                                        csvData.Columns.Add(dataColumn);
                                        selectedCol[selectedDataCount] = selectedDataCount;
                                        colType[selectedDataCount++] = match[0].Groups["Type"].Value;
                                    }
                                }

                                isColumnTitleRow = false;
                            }
                            else
                            {
                                string[] fieldData = line.Split(',');
                                object[] refactorData = new object[selectedDataCount];
                                //Making empty value as null
                                for (int i = 0; i < selectedDataCount; i++)
                                {
                                    switch (colType[i])
                                    {
                                        case "System.Byte[]":
                                            {
                                                var bytesString = fieldData[selectedCol[i]];
                                                if (string.IsNullOrEmpty(bytesString))
                                                {
                                                    refactorData[selectedCol[i]] = null;
                                                }
                                                else
                                                {
                                                    refactorData[selectedCol[i]] = GetBytesFromString(bytesString);
                                                }
                                                break;
                                            }
                                        default:
                                            {
                                                refactorData[selectedCol[i]]
                                                    = fieldData[selectedCol[i]] == "" ? null : fieldData[selectedCol[i]];
                                                break;
                                            }
                                    }
                                }
                                csvData.Rows.Add(refactorData);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
                return csvData;
            }
            async Task InsertDataIntoSQLServerUsingSQLBulkCopy(DataTable csvFileData, string tableName)
            {
                using (SqlConnection dbConnection = new SqlConnection(_connectionString))
                {
                    await dbConnection.OpenAsync();
                    using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                    {
                        s.DestinationTableName = tableName;
                        foreach (var column in csvFileData.Columns)
                        {
                            if (!string.IsNullOrEmpty(column.ToString()))
                                s.ColumnMappings.Add(column.ToString(), column.ToString());
                        }
                        await s.WriteToServerAsync(csvFileData);
                    }
                    dbConnection.Close();
                }
            }

            foreach (var path in csvFilePaths)
            {
                string tableName = Path.GetFileNameWithoutExtension(path);
                var datatable = await GetDataTabletFromCSVFile2(path);
                await InsertDataIntoSQLServerUsingSQLBulkCopy(datatable, tableName);
            }
        }

        public async Task ShowDatabaseTable()
        {
            using (SqlConnection cnn = new SqlConnection(_connectionString))
            {
                await cnn.OpenAsync();
                List<string> tables = new List<string>();
                var optionLst = new List<string>();
                {
                    DataTable dt = cnn.GetSchema("Tables");
                    foreach (DataRow row in dt.Rows)
                    {
                        string tablename = (string)row[2];
                        tables.Add(tablename);
                    }
                    optionLst = new MultiOptionWindow(tables.ToArray(), true)
                    {
                        Title = "Chọn table",
                        Owner = App.Current.ServerWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    }.ShowDialog();
                    if (optionLst.Count == 0) return;
                }

                foreach (var tableName in optionLst)
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM " + tableName))
                    {
                        cmd.Connection = cnn;
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.Connection = cnn;
                            sda.SelectCommand = cmd;

                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                DatabaseTableWindow window = new DatabaseTableWindow(dt)
                                {
                                    Title = tableName,
                                };
                                window.Show();
                            }
                        }
                    }
                }
            }

        }

        public async Task ShowDatabaseTable2()
        {
            string[] tables = _appDbContext.GetListTableName();
            var optionLst = new List<string>();
            {
                optionLst = new MultiOptionWindow(tables, true)
                {
                    Title = "Chọn table",
                    Owner = App.Current.ServerWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                }.ShowDialog();
                if (optionLst.Count == 0) return;
            }

            foreach (var tableName in optionLst)
            {
                DatabaseTableWindow window = new DatabaseTableWindow(_appDbContext.GetTableEnumerableByName2(tableName))
                {
                    Title = tableName,
                };
                window.Show();
            }

        }

        public async Task DropAllTable()
        {
            await _appDbContext.DropAllTable();
        }
    }
}
