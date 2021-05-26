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
    /// Interaction logic for MainWindowPage.xaml
    /// </summary>
    public partial class MainWindowPage : Page
    {
        private LoginWindow login;
        private string userName;
        private object loginContent;

        public MainWindowPage(LoginWindow loginWin, string userName)
        {
            
            this.login = loginWin;
            this.loginContent = loginWin.Content;
            this.userName = userName;
            InitializeComponent();
            welcometxt.Content = $"Welcome {userName}!";

        }

        private void StartConversation(object sender, RoutedEventArgs e)
        {
            login.Content = new ConversationPage(login,this, userName);
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            /*  LoginWindow window = new LoginWindow();
              this.login.Close();
              window.Show();*/
            this.login.Content = loginContent;

        }
    }
}
