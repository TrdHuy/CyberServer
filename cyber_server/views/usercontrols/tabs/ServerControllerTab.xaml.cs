using cyber_server.implements.http_server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    /// Interaction logic for ServerControllerTab.xaml
    /// </summary>
    public partial class ServerControllerTab : UserControl
    {
        public ServerControllerTab()
        {
            InitializeComponent();
        }

        private async void HandleButtonClickEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                switch (btn.Name)
                {
                    case "PART_StartServerButton":
                        {
                            await CyberHttpServer.Current.StartAsync();
                            break;
                        }
                    case "PART_StopServerButton":
                        {
                            CyberHttpServer.Current.Stop();
                            break;
                        }
                }
            }
        }

        private async void testRequest(object sender, RoutedEventArgs e)
        {
            var httpContent = new StringContent("HelloWorld222");

            httpContent.Headers.Add("h2sw-request-info", "GET_ALL_PLUGIN_DATA");

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("h2sw-request-info", "GET_ALL_PLUGIN_DATA");
            client.DefaultRequestHeaders.Add("GET_ALL_PLUGIN_DATA__MAXIMUM_AMOUNT", "10");
            client.DefaultRequestHeaders.Add("GET_ALL_PLUGIN_DATA__START_INDEX", "10");
            //var response = await client.PostAsync("http://107.127.131.89:8080/requestinfo", httpContent);
            var response = await client.GetAsync("http://107.127.131.89:8080/requestinfo");
             
            var responseContent = await response.Content.ReadAsStringAsync();
        }
    }
}
