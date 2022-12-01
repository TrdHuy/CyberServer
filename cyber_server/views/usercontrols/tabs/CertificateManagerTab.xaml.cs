using cyber_server.implements.cert_manager;
using cyber_server.implements.db_manager;
using cyber_server.implements.log_manager;
using cyber_server.implements.task_handler;
using cyber_server.view_models.list_view_item.certificate_item;
using cyber_server.views.usercontrols.others;
using cyber_server.views.windows.others;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace cyber_server.views.usercontrols.tabs
{
    /// <summary>
    /// Interaction logic for CertificateManagerTab.xaml
    /// </summary>
    public partial class CertificateManagerTab : UserControl
    {
        public ObservableCollection<CertificateItemViewModel> CertSource { get; set; }
            = new ObservableCollection<CertificateItemViewModel>();


        public CertificateManagerTab()
        {
            InitializeComponent();
            PART_CertsListView.ItemsSource = CertSource;
        }


        public async void OnTabReloaded(object sender, RoutedEventArgs e)
        {
            var handler = TaskHandlerManager.Current?.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
            await handler?.ExecuteTask(CurrentTaskManager.RELOAD_CERTIFICATE_FROM_DB_TASK_TYPE_KEY,
                mainFunc: async () =>
                {
                    CertSource.Clear();
                    await CyberDbManager.Current.RequestDbContextAsync((context) =>
                    {
                        foreach (var cert in context.Certificates)
                        {
                            CertSource.Add(new CertificateItemViewModel(cert));
                        }
                    });

                }
                , executeTime: 0
                , bypassIfSemaphoreNotAvaild: true);
        }

        private async void HandleCertManagementTabButtonEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            ServerLogManager.Current.D("On handling: " + btn.Name);
            var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);

            switch (btn.Name)
            {
                case "DeleteCertificateButton":
                    {
                        var vm = btn.DataContext as CertificateItemViewModel;
                        if (vm != null)
                        {
                            await handler.ExecuteTask(CurrentTaskManager.DELETE_CERTIFICATE_FROM_DB_TASK_TYPE_KEY
                                , mainFunc: async () =>
                                {
                                    var confirm = MessageBox.Show(messageBoxText: "Bạn có chắc xóa Certificate " + vm.CertKey + "?"
                                        , "Xác nhận"
                                        , button: MessageBoxButton.YesNo);
                                    if (confirm == MessageBoxResult.Yes)
                                    {
                                        await Task.Delay(1000);
                                        await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                        {
                                            context.Certificates.Remove(vm.RawModel);
                                            context.SaveChanges();
                                        });
                                        CertSource.Remove(vm);
                                    }
                                }
                               , executeTime: 0
                               , bypassIfSemaphoreNotAvaild: true);
                        }
                        break;
                    }
                case "ModifyCertificateButton":
                    {
                        var vm = btn.DataContext as CertificateItemViewModel;
                        if (vm != null)
                        {
                            await handler.ExecuteTask(CurrentTaskManager.SAVE_CERTIFICATE_TO_DB_TASK_TYPE_KEY
                                , mainFunc: async () =>
                                {
                                    var cmd = btn.Content;
                                    vm.ModifingModeEnable = !vm.ModifingModeEnable;
                                    btn.Content = vm.ModifingModeEnable ? "Save" : "Modify";
                                    if (cmd.Equals("Save"))
                                    {
                                        await Task.Delay(1000);
                                        await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                        {
                                            context.SaveChanges();
                                        });
                                    }
                                }
                               , executeTime: 0
                               , bypassIfSemaphoreNotAvaild: true);
                        }
                        break;
                    }
                case "ExtractCertificateButton":
                    {
                        var vm = btn.DataContext as CertificateItemViewModel;
                        if (vm != null)
                        {
                            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                            {
                                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                                {
                                    var selectedPath = fbd.SelectedPath;
                                    var certPath = selectedPath + "\\" + vm.RawModel.FileName;
                                    var confirm = true;
                                    if (File.Exists(certPath))
                                    {
                                        confirm = MessageBox.Show("File đã tồn tại, bạn có muốn ghi đè?"
                                             , "Xác nhận"
                                             , MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                                    }

                                    if (confirm)
                                    {
                                        using (var stream = File.Create(certPath))
                                        {
                                            await stream.WriteAsync(vm.RawModel.File, 0, vm.RawModel.File.Length);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case "PART_CheckKeyInvaildBtn":
                    {
                        if (!string.IsNullOrEmpty(PART_CertKeyTb.Text))
                        {
                            await handler.ExecuteTask(CurrentTaskManager.CHECK_VALIDATION_CERTIFICATE_TYPE_KEY,
                              mainFunc: async () =>
                              {
                                  Certificate certModel = null;
                                  var message = "";
                                  var certKey = PART_CertKeyTb.Text;

                                  await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                  {
                                      certModel = context
                                       .Certificates
                                       .Where(t => t.StringId.Equals(certKey))
                                       .FirstOrDefault();
                                      message = certModel != null ? "Invaild cert key!" : "Vaild cert key!";
                                  });

                                  MessageBox.Show(message);
                              }
                              , executeTime: 0
                              , bypassIfSemaphoreNotAvaild: true);
                        }

                        break;
                    }
                case "PART_BrowseBtn":
                    {
                        var ofd = new OpenFileDialog();
                        ofd.Filter = "PFX files (*.pfx)|*.pfx|Cert files (*.crt)|*.crt";
                        if (ofd.ShowDialog() == true)
                            PART_CertLocationTb.Text = ofd.FileName;
                        break;
                    }
                case "PART_ReloadListCert":
                    {
                        if (handler == null) return;
                        await handler.ExecuteTask(CurrentTaskManager.RELOAD_CERTIFICATE_FROM_DB_TASK_TYPE_KEY,
                           mainFunc: async () =>
                           {
                               CertSource.Clear();
                               await Task.Delay(1000);

                               await CyberDbManager.Current.RequestDbContextAsync((context) =>
                               {
                                   foreach (var cert in context.Certificates)
                                   {
                                       CertSource.Add(new CertificateItemViewModel(cert));
                                   }
                               });

                           }
                           , executeTime: 0
                           , bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "PART_SaveCertToDb":
                    {
                        var isShouldAddCertToDb = IsShouldAddCertToDb();
                        if (handler == null || !isShouldAddCertToDb) return;

                        await handler.ExecuteTask(CurrentTaskManager.SAVE_CERTIFICATE_TO_DB_TASK_TYPE_KEY,
                           mainFunc: async () =>
                           {
                               Certificate certModel = null;
                               var isCertExist = false;
                               var certKey = PART_CertKeyTb.Text;
                               var certLocation = PART_CertLocationTb.Text;

                               await CyberDbManager.Current.RequestDbContextAsync((context) =>
                               {
                                   certModel = context
                                    .Certificates
                                    .Where(t => t.StringId.Equals(certKey))
                                    .FirstOrDefault();
                                   isCertExist = certModel != null;
                               });

                               var certItemVM = new CertificateItemViewModel(new Certificate()
                               {
                                   StringId = certKey,
                               });

                               if (certModel != null)
                               {
                                   certItemVM = CertSource
                                    .Where(vm => vm.RawModel == certModel)
                                    .FirstOrDefault() ?? new CertificateItemViewModel(certModel);

                                   var confirm = MessageBox.Show(messageBoxText: "Cert key đã tồn tại, bạn có muốn ghi đè?"
                                       , "Xác nhận"
                                       , button: MessageBoxButton.YesNo);
                                   if (confirm == MessageBoxResult.No)
                                   {
                                       return;
                                   }
                               }

                               X509Certificate2 cert = null;
                               String certPassword = null;
                               if (System.IO.Path.GetExtension(certLocation) == ".pfx")
                               {
                                   var eBW = new EditBoxWindow("password: "
                                   , ""
                                   , checkConditionSatisfyToCloseWindow: (pass) =>
                                   {
                                       try
                                       {
                                           cert = new X509Certificate2(certLocation, pass, X509KeyStorageFlags.DefaultKeySet);
                                           certPassword = pass;
                                       }
                                       catch { }
                                       if (cert == null)
                                       {
                                           MessageBox.Show("Sai mật khẩu, nhập lại!");
                                           return false;
                                       }
                                       return true;
                                   }
                                   , owner: App.Current.ServerWindow
                                   , isPasswordBox: true);
                                   eBW.Show();

                               }
                               else
                               {
                                   cert = new X509Certificate2(certLocation);
                               }

                               if (cert == null) return;

                               certItemVM.Expiration = cert?.GetExpirationDateString();
                               await CyberDbManager.Current.RequestDbContextAsync((context) =>
                               {
                                   certItemVM.RawModel.File = File.ReadAllBytes(certLocation);
                                   certItemVM.RawModel.FileName = System.IO.Path.GetFileName(certLocation);
                                   certItemVM.RawModel.Description = PART_CertDesTb.Text ?? "";
                                   certItemVM.RawModel.FileType = System.IO.Path.GetExtension(certLocation);
                                   certItemVM.RawModel.Location = certLocation;
                                   if(certPassword != null)
                                   {
                                       certItemVM.RawModel.Password = DESCryptoManager.EncryptTextToMemory(certPassword);
                                   }

                                   if (!isCertExist)
                                   {
                                       context.Certificates.Add(certItemVM.RawModel);
                                       CertSource.Add(certItemVM);
                                   }
                                   context.SaveChanges();
                               });

                           }
                           , executeTime: 0
                           , bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "PART_RefreshCertTab":
                    {
                        PART_CertKeyTb.Text = "";
                        PART_CertDesTb.Text = "";
                        PART_CertLocationTb.Text = "";
                        break;
                    }
                
            }

        }

        private bool IsShouldAddCertToDb()
        {
            if (string.IsNullOrEmpty(PART_CertKeyTb.Text))
            {
                MessageBox.Show("Cert key không được bỏ trống!");
                return false;
            }

            if (string.IsNullOrEmpty(PART_CertLocationTb.Text))
            {
                MessageBox.Show("Hãy chọn cert cần thêm!");
                return false;
            }
            else if (!File.Exists(PART_CertLocationTb.Text))
            {
                MessageBox.Show("File cert không tồn tại!");
                return false;
            }
            else if (System.IO.Path.GetExtension(PART_CertLocationTb.Text) != ".pfx"
                && System.IO.Path.GetExtension(PART_CertLocationTb.Text) != ".crt")
            {
                MessageBox.Show("File cert không đúng định dạng!");
                return false;
            }


            return true;
        }
    }
}
