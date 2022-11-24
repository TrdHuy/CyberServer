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
    /// Interaction logic for MultiOptionWindow.xaml
    /// </summary>
    public partial class MultiOptionWindow : Window
    {
        public MultiOptionWindow(string[] options, bool isSelectableMultiOption = false)
        {
            InitializeComponent();

            PART_OptionBox.Children.Clear();

            foreach (var option in options)
            {
                ButtonBase child = null;
                if (isSelectableMultiOption)
                {
                    child = new CheckBox();
                }
                else
                {
                    child = new RadioButton();
                }

                var margin = new Thickness();
                margin.Bottom = 5;
                margin.Left = 10;
                margin.Right = 15;
                margin.Top = 20;
                child.Margin = margin;
                var textContent = new TextBlock();
                textContent.Text = option;
                textContent.MaxHeight = 40;
                textContent.TextTrimming = TextTrimming.CharacterEllipsis;
                textContent.TextWrapping = TextWrapping.Wrap;
                child.Content = textContent;

                PART_OptionBox.Children.Add(child);
            }
        }

        public new List<string> Show()
        {
            base.ShowDialog();
            var selectedList = new List<string>();
            foreach (var child in PART_OptionBox.Children)
            {
                var btnbase = child as ToggleButton;
                var textblock = btnbase?.Content as TextBlock;
                if (textblock != null && (btnbase.IsChecked ?? false))
                {
                    selectedList.Add(textblock.Text);
                }
            }
            return selectedList;
        }

        public new List<string> ShowDialog()
        {
            base.ShowDialog();
            var selectedList = new List<string>();
            foreach (var child in PART_OptionBox.Children)
            {
                var btnbase = child as ToggleButton;
                var textblock = btnbase?.Content as TextBlock;
                if (textblock != null && (btnbase.IsChecked ?? false))
                {
                    selectedList.Add(textblock.Text);
                }
            }
            return selectedList;
        }

        private void HandleSubmitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HandleCancelClick(object sender, RoutedEventArgs e)
        {
            PART_OptionBox.Children.Clear();
            this.Close();
        }
    }
}
