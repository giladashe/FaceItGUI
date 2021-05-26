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
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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
                //{ "salt", salt},
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


        /*private string HashPassword(string password)
        {
            // generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");

            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            Console.WriteLine($"Hashed: {hashed}");
            return hashed;
        }*/


        /*private bool ArePasswordsEqual(string inputPassword, string dbPassword)
        {
            Fetch the stored value
            // string savedPasswordHash = DBContext.GetUser(u => u.UserName == user).Password;
             Extract the bytes
            byte[] hashBytes = Convert.FromBase64String(dbPassword);
            Get the salt
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            Compute the hash on the password the user entered
           var pbkdf2 = Convert.ToBase64String(KeyDerivation.Pbkdf2(
               password: password,
               salt: salt,
               prf: KeyDerivationPrf.HMACSHA256,
               iterationCount: 10000,
               numBytesRequested: 256 / 8));
            byte[] hash = pbkdf2.GetBytes(20);
            Compare the results
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    throw new UnauthorizedAccessException()
        }*/
    }
}



//  hash(password + salt)
// username password - login == salt