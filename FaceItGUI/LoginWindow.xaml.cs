using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Http;
using System.Net;

namespace FaceItGUI
{
    /// <summary>
    /// Interaction logic for SnippingWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private int Port;
        private string Ip;

        private static readonly HttpClient client = new HttpClient();


        public LoginWindow()
        {
            InitializeComponent();
            this.Ip = ConfigurationManager.AppSettings["HttpIp"];
            this.Port = Convert.ToInt32(ConfigurationManager.AppSettings["HttpPort"]);
        }


        public async void LoginClick(object sender, RoutedEventArgs e)
        {
            //take username and password
            string userName = txtUserName.Text;
            string password = txtPassword.Password;

            //todo: erase

            //CleanFields();
            //this.Content = new MainWindowPage(this, userName);
            //return;


            if (userName == string.Empty || password == string.Empty)
            {
                errorTxt.Content = "Missing fields!";
                return;
            }
            string httpLoginRequest = "http://" + this.Ip + ":" + this.Port + "/login?username=" + userName + "&password=" + password;
            try
            {
                var response = await client.GetAsync(httpLoginRequest);

                var responseString = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        if (responseString == "success")
                        {
                            CleanFields();
                            this.Content = new MainWindowPage(this, userName);
                        }
                        else
                        {
                            errorTxt.Content = "Wrong username/password!";
                        }
                        break;
                    case HttpStatusCode.BadRequest:
                        errorTxt.Content = "DB connection problem";
                        break;
                    default:
                        errorTxt.Content = "Connection Problem";
                        break;
                }
            }
            catch
            {
                errorTxt.Content = "Connection Problem";
            }

        }


        public void RegisterClick(object sender, RoutedEventArgs e)
        {
            CleanFields();
            this.Content = new RegisterPage(this);

        }

        private void CleanFields()
        {
            txtUserName.Text = string.Empty;
            txtPassword.Password = string.Empty;
            errorTxt.Content = string.Empty;
        }

        public void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
