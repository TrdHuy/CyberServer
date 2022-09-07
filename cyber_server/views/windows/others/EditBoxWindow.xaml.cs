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
    /// Interaction logic for EditBoxWindow.xaml
    /// </summary>
    public partial class EditBoxWindow : Window
    {
        private Func<string, bool> _checkConditionSatisfyToCloseWindow;
        private bool _saveButtonCallFlag = false;
        public bool IsCanceled { get; private set; }
        public EditBoxWindow(string uneditableText
            , string editableText
            , Func<string, bool> checkConditionSatisfyToCloseWindow = null)
        {
            InitializeComponent();
            PART_UneditText.Text = uneditableText;
            PART_EditableText.Text = editableText;
            _checkConditionSatisfyToCloseWindow = checkConditionSatisfyToCloseWindow;
        }

        public new string Show()
        {
            base.ShowDialog();
            return PART_UneditText.Text + PART_EditableText.Text;
        }

        public new string ShowDialog()
        {
            base.ShowDialog();
            return PART_UneditText.Text + PART_EditableText.Text;
        }

        private void HandleWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_saveButtonCallFlag)
            {
                if (_checkConditionSatisfyToCloseWindow?.Invoke(PART_EditableText.Text) ?? true)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                IsCanceled = true;
            }

        }

        private void HandleSaveButton(object sender, RoutedEventArgs e)
        {
            _saveButtonCallFlag = true;
            this.Close();
            _saveButtonCallFlag = false;
        }
    }
}
