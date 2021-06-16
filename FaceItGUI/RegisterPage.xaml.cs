using System;
using System.Collections.Generic;
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
using System.Net.Http;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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
                    errorTxt.Text = "All fields are mandatory!";
                }));
                return;
            }

            await Dispatcher.BeginInvoke(new Action(delegate ()
            {
                errorTxt.Text = string.Empty;
            }));
            //string salt = Hash.CreateSalt();
            //string hashedPassword = Hash.CreateHashedPassword(password, salt);
            var values = new Dictionary<string, string>
            {
                { "userName", userName },
                { "password", password },
                { "email", email }
            };
            //WebRequestHandler handler = new WebRequestHandler();
            //X509Certificate certificate = X509Certificate.CreateFromCertFile("");
            //handler.ClientCertificates.Add(certificate);
            //HttpClient client = new HttpClient(handler);
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
                                errorTxt.Text = "This userName already exists in the system";
                            }
                            else if (responseString == "mail exists")
                            {
                                errorTxt.Text = "This mail already exists in the system";
                            }
                            else if (responseString == "db failure")
                            {
                                errorTxt.Text = "Failure with connection to DB";
                            }
                            else
                            {
                                errorTxt.Text = responseString;
                            }
                            break;
                        default:
                            errorTxt.Text = "Problem with connection to server";
                            break;
                    }
                }));
            }
            catch
            {
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    errorTxt.Text = "Problem with connection to server";
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

    }
}