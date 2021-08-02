using System;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using LiveCharts;
using LiveCharts.Wpf;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Net;

namespace FaceItGUI.statistics
{
    /// <summary>
    /// Interaction logic for StatisticsDashboard.xaml
    /// </summary>
    public partial class StatisticsDashboard : Page
    {
        private MainWindowPage mainWinPage;
        private readonly string userName;

        private LoginWindow parentWindow;
        private object loginContent;
        // private StatisticsHome statisticsHomePage;
        private string time;
        private int Port;
        private string Ip;
        private static readonly HttpClient client = new HttpClient();

        private string positivePercents = "0"; // HappySad
        private string negativePercents = "0";
        private string userMatchPercent = "0"; // UserMatch


        public StatisticsDashboard(LoginWindow loginWindow, MainWindowPage main, string userName)
        {
            InitializeComponent();
            this.parentWindow = loginWindow;
            this.loginContent = loginWindow.Content;
            this.mainWinPage = main;
            this.userName = userName;
            this.Ip = ConfigurationManager.AppSettings["HttpIp"];
            this.Port = Convert.ToInt32(ConfigurationManager.AppSettings["HttpPort"]);


            HappySadSeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "positive",
                    Values = new ChartValues<double> { 0 } //positivePercents, Convert.ToDouble("66.3")
                }
            };

            HappySadSeriesCollection.Add(new ColumnSeries
            {
                Title = "negative",
                Values = new ChartValues<double> { 0 } //negativePercents
            });


            MatchUserSeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "match",
                    Values = new ChartValues<double> { 0 } 
                }
            };


            BarLabels = new[] {""};
            Formatter = value => value.ToString("N");

            DataContext = this;

        }


        private void Exit(object sender, RoutedEventArgs e)
        {
            parentWindow.Content = this.loginContent;
        }

        /*private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        }*/


        public SeriesCollection HappySadSeriesCollection { get; set; }

        public SeriesCollection MatchUserSeriesCollection { get; set; }

        public string[] BarLabels { get; set; }

        public Func<double, string> Formatter { get; set; }

        private void RadioButton_Last_Call_Checked(object sender, RoutedEventArgs e)
        {
            time = "last_call";
            UpdateCharts();
        }

        private void RadioButton_Last_Week_Checked(object sender, RoutedEventArgs e)
        {
            time = "last_week";
            UpdateCharts();
        }

        private void RadioButton_Last_Month_Checked(object sender, RoutedEventArgs e)
        {
            time = "last_month";
            UpdateCharts();
        }

        private void UpdateCharts()
        {
            getHappySad();
            getUserMatch();
        }

        private async void getHappySad()  
        {
            string getHappySadhRequest = "http://" + this.Ip + ":" + this.Port + "/statistics/user/happy_sad?user_name=" + userName + "&time=" + time;
            try
            {
                var response = await client.GetAsync(getHappySadhRequest);
                var responseString = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseString);
                positivePercents = json["positive_percents"].ToString();
                negativePercents = json["negative_percents"].ToString();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    case HttpStatusCode.BadRequest:
                        //errorTxt.Content = "DB connection problem"; TODO
                        break;
                    default:
                        //errorTxt.Content = "Problem with connection to server";
                        break;
                }
            }
            catch
            {
                // errorTxt.Content = "Problem with connection to server";
            }

            HappySadSeriesCollection.Clear();
            HappySadSeriesCollection.Add(new ColumnSeries
            {
                Title = "positive",
                Values = new ChartValues<double> { Convert.ToDouble(positivePercents) } //negativePercents
            });

            HappySadSeriesCollection.Add(new ColumnSeries
            {
                Title = "negative",
                Values = new ChartValues<double> { Convert.ToDouble(negativePercents) } //negativePercents
            });

        }

        private async void getUserMatch()
        {
            string getUserMatchRequest = "http://" + this.Ip + ":" + this.Port + "/statistics/user/match?user_name=" + userName + "&time=" + time;
            try
            {
                var response = await client.GetAsync(getUserMatchRequest);
                var responseString = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseString);
                userMatchPercent = json["percents"].ToString();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    case HttpStatusCode.BadRequest:
                        //errorTxt.Content = "DB connection problem"; TODO
                        break;
                    default:
                        //errorTxt.Content = "Problem with connection to server";
                        break;
                }
            }
            catch
            {
                // errorTxt.Content = "Problem with connection to server";
            }

            MatchUserSeriesCollection.Clear();
            MatchUserSeriesCollection.Add(new ColumnSeries
            {
                Title = "match",
                Values = new ChartValues<double> { Convert.ToDouble(userMatchPercent) } //negativePercents
            });

        }

    }
}
