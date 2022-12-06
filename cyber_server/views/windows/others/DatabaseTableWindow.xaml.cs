using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace cyber_server.views.windows.others
{
    /// <summary>
    /// Interaction logic for DatabaseTableWindow.xaml
    /// </summary>
    public partial class DatabaseTableWindow : Window
    {
        private object threadSafeLock = new object();
        private IEnumerable sourceCache;
        public DatabaseTableWindow(IEnumerable source)
        {
            InitializeComponent();
            sourceCache = source;
            BindingOperations.EnableCollectionSynchronization(sourceCache, threadSafeLock);
            var collectionChanged = source as INotifyCollectionChanged;
            collectionChanged.CollectionChanged += OnSourceCollectionChanged;
            PART_MainDataGrid.AutoGeneratingColumn += PART_MainDataGrid_AutoGeneratingColumn;
            PART_MainDataGrid.ItemsSource = source;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            BindingOperations.DisableCollectionSynchronization(sourceCache);
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
           
        }

        private void PART_MainDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var isColumnEditable = (e.PropertyDescriptor as PropertyDescriptor)
                .Attributes?
                .OfType<EditableAttribute>()?
                .FirstOrDefault()?
                .AllowEdit ?? true;
            if (!isColumnEditable)
            {
                e.Column.IsReadOnly = true;
            }
        }
        public DatabaseTableWindow(DataTable source)
        {
            InitializeComponent();
            PART_MainDataGrid.ItemsSource = source.DefaultView;
        }
    }
}
