using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaceItGUI
{
    /// <summary>
    /// Interaction logic for MainWindowPage.xaml
    /// </summary>
    public partial class MainWindowPage : Page
    {
        private LoginWindow login;
        private string userName;
        private object loginContent;
        private readonly int Port;
        private readonly string Ip;
        private static readonly HttpClient client = new HttpClient();

        public MainWindowPage(LoginWindow loginWin, string userName)
        {
            
            this.login = loginWin;
            this.loginContent = loginWin.Content;
            this.userName = userName;
            this.Ip = ConfigurationManager.AppSettings["HttpIp"];
            this.Port = Convert.ToInt32(ConfigurationManager.AppSettings["HttpPort"]);
            InitializeComponent();
            welcometxt.Content = $"Welcome {userName}!";

        }

        private async void StartConversation(object sender, RoutedEventArgs e)
        {
            string httpLoginRequest = "http://" + this.Ip + ":" + this.Port + "/start?username=" + this.userName;
            try
            {
                var response = await client.GetAsync(httpLoginRequest);

                var responseString = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                            login.Content = new ConversationPage(login, this, userName);
                        break;
                    default:
                        errorTxt.Content = "Problem with connection to server";
                        break;
                }
            }
            catch
            {
                errorTxt.Content = "Problem with connection to server";
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.login.Content = loginContent;
        }
    }
}
