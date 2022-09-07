using System;
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
    /// Interaction logic for TextWindow.xaml
    /// </summary>
    public partial class TextWindow : Window
    {
        public TextWindow(string initText,
            bool isReadOnly = true)
        {
            InitializeComponent();
            PART_MainTextBox.Text = initText;
            PART_MainTextBox.IsReadOnly = isReadOnly;
        }

        public new string Show()
        {
            base.ShowDialog();
            return PART_MainTextBox.Text;
        }

        public new string ShowDialog()
        {
            base.ShowDialog();
            return PART_MainTextBox.Text;
        }
    }
}
