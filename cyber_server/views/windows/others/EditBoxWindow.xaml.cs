using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private bool _isPasswordBox;
        public bool IsCanceled { get; private set; }

        public EditBoxWindow(string uneditableText
            , string editableText
            , Func<string, bool> checkConditionSatisfyToCloseWindow = null
            , Window owner = null
            , bool isPasswordBox = false)
        {
            InitializeComponent();
            _isPasswordBox = isPasswordBox;
            PART_EditablePassword.Visibility = isPasswordBox ? Visibility.Visible : Visibility.Collapsed;
            PART_EditableText.Visibility = isPasswordBox ? Visibility.Collapsed : Visibility.Visible;
            PART_YesButton.Content = isPasswordBox ? "Next" : "Save";

            PART_UneditText.Text = uneditableText;
            PART_EditableText.Text = editableText;
            PART_EditablePassword.Password = editableText;
            _checkConditionSatisfyToCloseWindow = checkConditionSatisfyToCloseWindow;
            if (owner != null)
            {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Enter)
            {
                PART_YesButton.Focus();
                e.Handled = true;
            }
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.Enter)
            {
                PART_YesButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                e.Handled = true;
            }
        }

        public new string Show()
        {
            base.ShowDialog();
            return _isPasswordBox
                ? PART_EditablePassword.Password
                : PART_UneditText.Text + PART_EditableText.Text;
        }

        public new string ShowDialog()
        {
            base.ShowDialog();
            return _isPasswordBox
                ? PART_EditablePassword.Password
                : PART_UneditText.Text + PART_EditableText.Text;
        }

        private void HandleWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_saveButtonCallFlag)
            {
                if (_checkConditionSatisfyToCloseWindow?
                    .Invoke(_isPasswordBox 
                        ? PART_EditablePassword.Password 
                        : PART_EditableText.Text) ?? true)
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
