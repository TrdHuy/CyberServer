using cyber_server.views.windows.others;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace cyber_server.views.extension
{
    internal class ShowVersionHistoryButton : ToggleButton
    {
        private VersionHistoryWindow _windowCache;

        #region VersionHistorySource
        public static readonly DependencyProperty VersionHistorySourceProperty =
            DependencyProperty.Register(
                "VersionHistorySource",
                typeof(IEnumerable),
                typeof(ShowVersionHistoryButton),
                new PropertyMetadata(default(IEnumerable), new PropertyChangedCallback(OnVersionHistorySourceChangedCallback)));

        private static void OnVersionHistorySourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ShowVersionHistoryButton;
            ctrl?._windowCache?.SetVersionHistorySource((IEnumerable)e.NewValue);
        }

        public IEnumerable VersionHistorySource
        {
            get => (IEnumerable)GetValue(VersionHistorySourceProperty);
            set => SetValue(VersionHistorySourceProperty, value);
        }
        #endregion

        protected override void OnClick()
        {
            base.OnClick();
            var isShouldOpenVersionHistoryWindow = IsChecked ?? false;
            if (isShouldOpenVersionHistoryWindow)
            {
                var vhw = new VersionHistoryWindow(VersionHistorySource);
                vhw.Closed += (s, arg) =>
                {
                    IsChecked = false;
                };
                vhw.Owner = App.Current.ServerWindow;
                vhw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _windowCache = vhw;
                _windowCache.Show();
            }
            else
            {
                _windowCache.Close();
            }
        }
    }
}
