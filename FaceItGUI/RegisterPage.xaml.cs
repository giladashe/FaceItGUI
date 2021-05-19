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

namespace FaceItGUI
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        private LoginWindow loginWin;
        private object loginContent;
        public RegisterPage(LoginWindow logWin)
        {
            this.loginWin = logWin;
            this.loginContent = logWin.Content;
            InitializeComponent();
            this.loginWin.Content = this.Content;
        }


        public void RegisterUser(object sender, RoutedEventArgs e)
        {
            //todo: check fields and send to Israel
            BackToLogin();
        }

        public void Exit(object sender, RoutedEventArgs e)
        {
            BackToLogin();
        }

        public void BackToLogin() {
            this.loginWin.Content = loginContent;
            this.loginWin.txtUserName.Text = "";
            this.loginWin.txtPassword.Password = "";
        }
    }
}
