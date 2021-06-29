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
    /// Interaction logic for statistics_home.xaml
    /// </summary>
    public partial class StatisticsHome : Page
    {

        private LoginWindow login;
        private object loginContent;
        private MainWindowPage mainWinPage;
        private readonly string userName;

        public StatisticsHome(LoginWindow loginWindow, MainWindowPage main, string userName)
        {
            InitializeComponent();
            this.login = loginWindow;
            this.loginContent = loginWindow.Content;
            this.mainWinPage = main;
            this.userName = userName;
        }

        private void getUserMatch(object sender, RoutedEventArgs e)
        {
            login.Content = new FaceItGUI.statistics.MatchUser(login, this, userName);
        }
        private void others(object sender, RoutedEventArgs e)
        {
            login.Content = new FaceItGUI.statistics.OthersPositive(login, this, userName);
        }

        private void compareHappySad(object sender, RoutedEventArgs e)
        {
            login.Content = new FaceItGUI.statistics.HappySad(login, this, userName);
        }

        private void getAllEmotions(object sender, RoutedEventArgs e)
        {
            login.Content = new FaceItGUI.statistics.TotalEmotions(login, this, userName);
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            login.Content = this.loginContent;
        }

    }
}
