using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Interaction logic for VersionHistoryWindow.xaml
    /// </summary>
    public partial class VersionHistoryWindow : Window
    {
        public VersionHistoryWindow(IEnumerable versionHistorySource)
        {
            InitializeComponent();
            PART_VersionHistoryListView.ItemsSource = versionHistorySource;
        }

        public void SetVersionHistorySource(IEnumerable source)
        {
            PART_VersionHistoryListView.ItemsSource = null;
            PART_VersionHistoryListView.ItemsSource = source;
        }
    }
}
