using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;



namespace FaceItGUI.statistics
{
    /// <summary>
    /// Interaction logic for MatchUser.xaml
    /// </summary>
    public partial class TotalEmotions : Page
    {
        private LoginWindow parentWindow;
        private object loginContent;
        private StatisticsHome statisticsHomePage;
        private readonly string userName;
        private string time;
        private int Port;
        private string Ip;
        private static readonly HttpClient client = new HttpClient();

        public TotalEmotions(LoginWindow loginWindow, StatisticsHome statisticsHomePage, string userName)
        {
            InitializeComponent();
            this.parentWindow = loginWindow;
            this.loginContent = loginWindow.Content;
            this.statisticsHomePage = statisticsHomePage;
            this.userName = userName;
            this.Ip = ConfigurationManager.AppSettings["HttpIp"];
            this.Port = Convert.ToInt32(ConfigurationManager.AppSettings["HttpPort"]);
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.parentWindow.Content = this.loginContent;
        }

        private async void getTotalEmotions(object sender, RoutedEventArgs e)
        {
            string getTotalEmotionsRequest = "http://" + this.Ip + ":" + this.Port + "/statistics/user/emotions?user_name=" + userName + "&time=" + time;
            try
            {
                var response = await client.GetAsync(getTotalEmotionsRequest);
                var responseString = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseString);
                var happyPercents = json["happy_percents"].ToString();
                var neutralPercents = json["neutral_percents"].ToString();
                var sadPercents = json["sad_percents"].ToString();
                var surprisePercents = json["surprise_percents"].ToString();
                var angryPercents = json["angry_percents"].ToString();
                var disgustPercents = json["disgust_percents"].ToString();
                var fearPercents = json["fear_percents"].ToString();


                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        resultTxt.Content = $"happy: {happyPercents}%, neutral: {neutralPercents}%, sad: {sadPercents}%,\n" +
                            $" surprise: {surprisePercents}%, angry: {angryPercents}%, disgust: {disgustPercents}%\n, fear: {fearPercents}%";
                        break;
                    case HttpStatusCode.BadRequest:
                        errorTxt.Content = "DB connection problem";
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
            if (text == "Last Call")
            {
                time = "last_call";
            }
            else if (text == "Last Week")
            {
                time = "last_week";
            }
            else if (text == "Last Month")
            {
                time = "last_month";
            }
        }

    }
}
