using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.Http;
using System.Configuration;
using System.Net;

namespace FaceItGUI
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        private readonly LoginWindow loginWin;
        private readonly object loginContent;
        private readonly int Port;
        private readonly string Ip;
        private static readonly HttpClient client = new HttpClient();

        public RegisterPage(LoginWindow logWin)
        {
            this.loginWin = logWin;
            this.loginContent = logWin.Content;
            InitializeComponent();
            this.loginWin.Content = this.Content;
            this.Ip = ConfigurationManager.AppSettings["HttpIp"];
            this.Port = Convert.ToInt32(ConfigurationManager.AppSettings["HttpPort"]);
        }


        public async void RegisterUser(object sender, RoutedEventArgs e)
        {
            string userName = txtUserName.Text;
            string password = txtPassword.Password;
            string email = txtEmail.Text;

            if (userName == string.Empty || password == string.Empty || email == string.Empty)
            {
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    errorTxt.Content = "All fields are mandatory!";
                }));
                return;
            }

            await Dispatcher.BeginInvoke(new Action(delegate ()
            {
                errorTxt.Content = string.Empty;
            }));
            var values = new Dictionary<string, string>
            {
                { "userName", userName },
                { "password", password },
                { "email", email }
            };
            var content = new FormUrlEncodedContent(values);
            string httpRegisterPost = "http://" + this.Ip + ":" + this.Port + "/register";
            try
            {
                var response = await client.PostAsync(httpRegisterPost, content);
                var responseString = await response.Content.ReadAsStringAsync();
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            BackToLogin();
                            break;
                        case HttpStatusCode.BadRequest:
                            if (responseString == "userName exists")
                            {
                                errorTxt.Content = "This userName already exists in the system";
                            }
                            else if (responseString == "mail exists")
                            {
                                errorTxt.Content = "This mail already exists in the system";
                            }
                            else if (responseString == "db failure")
                            {
                                errorTxt.Content = "Failure with connection to DB";
                            }
                            else
                            {
                                errorTxt.Content = responseString;
                            }
                            break;
                        default:
                            errorTxt.Content = "Problem with connection to server";
                            break;
                    }
                }));
            }
            catch
            {
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    errorTxt.Content = "Problem with connection to server";
                }));
            }
        }

        public void Exit(object sender, RoutedEventArgs e)
        {
            BackToLogin();
        }

        public void BackToLogin()
        {
            this.loginWin.Content = loginContent;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.loginWin.DragMove();
            }
        }
    }
}