using System;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using LiveCharts;
using LiveCharts.Wpf;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Net;
using LiveCharts.Defaults;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Input;

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
        private string time = string.Empty;
        private int Port;
        private string Ip;
        private static readonly HttpClient client = new HttpClient();

        private string positivePercents = "0"; // HappySad
        private string negativePercents = "0";
        private string userMatchPercent = "0"; // UserMatch
        private string othersPositivePercent = "0"; // OthersPositive
        private string happyPercents = "0"; // TotalEmotions
        private string neutralPercents = "0";
        private string sadPercents = "0";
        private string surprisePercents = "0";
        private string angryPercents = "0";
        private string disgustPercents = "0";
        private string fearPercents = "0";


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

            OthersPositiveSeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "positive",
                    Values = new ChartValues<double> { 0 }
                }
            };

            TotalEmotionsSeriesCollection = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "happy",
                    Values = new ChartValues<ObservableValue> {new ObservableValue(0) },
                    DataLabels = true
                } ,
                new PieSeries
                {
                    Title = "neutral",
                    Values = new ChartValues<ObservableValue> {new ObservableValue(0) },
                    DataLabels = true
                } ,
                new PieSeries
                {
                    Title = "sad",
                    Values = new ChartValues<ObservableValue> {new ObservableValue(0) },
                    DataLabels = true
                } ,
                new PieSeries
                {
                    Title = "surprise",
                    Values = new ChartValues<ObservableValue> {new ObservableValue(0) },
                    DataLabels = true
                } ,
                new PieSeries
                {
                    Title = "angry",
                    Values = new ChartValues<ObservableValue> {new ObservableValue(0) },
                    DataLabels = true
                } ,
                new PieSeries
                {
                    Title = "disgust",
                    Values = new ChartValues<ObservableValue> {new ObservableValue(0) },
                    DataLabels = true
                } ,
                new PieSeries
                {
                    Title = "fear",
                    Values = new ChartValues<ObservableValue> {new ObservableValue(0) },
                    DataLabels = true
                }
            };

            BarLabels = new[] {""};
            Formatter = value => value.ToString("N");

            DataContext = this;

/*            System.Windows.Point topLeft = dummy_button.PointToScreen(new System.Windows.Point(0, 0));
            double x = this.parentWindow.Left; //System.Windows.Point
            double y = this.parentWindow.Top;
            var pass = 1;
*/
            //System.Windows.Forms.Screen r = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(parentWindow).Handle);
            /*            double Left = SystemParameters.VirtualScreenLeft;
                        double Top = SystemParameters.VirtualScreenTop;
                        double ScreenWidth = SystemParameters.VirtualScreenWidth;
                        double ScreenHeight = SystemParameters.VirtualScreenHeight;
            */
            //var bounds = Screen.PrimaryScreen.Bounds;
            //lastCallButton.IsChecked = true;

        }


        private void BachToMenu(object sender, RoutedEventArgs e)
        {
            this.parentWindow.Content = this.mainWinPage;
        }

        public SeriesCollection HappySadSeriesCollection { get; set; }

        public SeriesCollection MatchUserSeriesCollection { get; set; }

        public SeriesCollection OthersPositiveSeriesCollection { get; set; }

        public SeriesCollection TotalEmotionsSeriesCollection { get; set; }


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
            getOthersPositive();
            getTotalEmotions();
        }

        private async void getHappySad()  
        {
            string getHappySadhRequest = "http://" + this.Ip + ":" + this.Port + "/statistics/user/happy_sad?user_name=" + userName + "&time=" + time;
            try
            {
                var response = await client.GetAsync(getHappySadhRequest);
                var responseString = await response.Content.ReadAsStringAsync();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        errorTxt.Text = "";
                        JObject json = JObject.Parse(responseString);
                        positivePercents = json["positive_percents"].ToString();
                        negativePercents = json["negative_percents"].ToString();
                        break;
                    case HttpStatusCode.BadRequest:
                        errorTxt.Text = "DB connection problem";
                        break;
                    default:
                        errorTxt.Text = "Problem with connection to server";
                        break;
                }
            }
            catch
            {
                errorTxt.Text = "Problem with connection to server";
                return;
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

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        errorTxt.Text = "";
                        if (responseString == "no checks has been detected")
                        {
                            userMatchPercent = "0";
                        }
                        else
                        {
                            JObject json = JObject.Parse(responseString);
                            userMatchPercent = json["percents"].ToString();
                        }
                        break;
                    case HttpStatusCode.BadRequest:
                        errorTxt.Text = "DB connection problem"; 
                        break;
                    default:
                        errorTxt.Text = "Problem with connection to server";
                        break;
                }
            }
            catch
            {
                errorTxt.Text = "Problem with connection to server";
                return;
            }
            MatchUserSeriesCollection.Clear();
            MatchUserSeriesCollection.Add(new ColumnSeries
            {
                Title = "match",
                Values = new ChartValues<double> { Convert.ToDouble(userMatchPercent) } //negativePercents
            });

        }

        private async void getOthersPositive()
        {
            string getOthersPositiveRequest = "http://" + this.Ip + ":" + this.Port + "/statistics/others?user_name=" + userName + "&time=" + time;
            try
            {
                var response = await client.GetAsync(getOthersPositiveRequest);
                var responseString = await response.Content.ReadAsStringAsync();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        errorTxt.Text = "";
                        JObject json = JObject.Parse(responseString);
                        othersPositivePercent = json["percents"].ToString();
                        break;
                    case HttpStatusCode.BadRequest:
                        errorTxt.Text = "DB connection problem"; 
                        break;
                    default:
                        errorTxt.Text = "Problem with connection to server";
                        break;
                }
            }
            catch
            {
                errorTxt.Text = "Problem with connection to server";
                return;
            }
            OthersPositiveSeriesCollection.Clear();
            OthersPositiveSeriesCollection.Add(new ColumnSeries
            {
                Title = "postive",
                Values = new ChartValues<double> { Convert.ToDouble(othersPositivePercent) } 
            });

        }

        private async void getTotalEmotions()
        {
            string getTotalEmotionsRequest = "http://" + this.Ip + ":" + this.Port + "/statistics/user/emotions?user_name=" + userName + "&time=" + time;
            try
            {
                var response = await client.GetAsync(getTotalEmotionsRequest);
                var responseString = await response.Content.ReadAsStringAsync();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        errorTxt.Text = "";
                        JObject json = JObject.Parse(responseString);
                        happyPercents = json["happy_percents"].ToString();
                        neutralPercents = json["neutral_percents"].ToString();
                        sadPercents = json["sad_percents"].ToString();
                        surprisePercents = json["surprise_percents"].ToString();
                        angryPercents = json["angry_percents"].ToString();
                        disgustPercents = json["disgust_percents"].ToString();
                        fearPercents = json["fear_percents"].ToString();
                        break;
                    case HttpStatusCode.BadRequest:
                        errorTxt.Text = "DB connection problem"; 
                        break;
                    default:
                        errorTxt.Text = "Problem with connection to server";
                        break;
                }
            }
            catch
            {
                errorTxt.Text = "Problem with connection to server";
                return;
            }
            TotalEmotionsSeriesCollection.Clear();
            TotalEmotionsSeriesCollection.Add(new PieSeries
            {
                Title = "happy",
                Values = new ChartValues<ObservableValue> { new ObservableValue(Convert.ToDouble(happyPercents)) },
                DataLabels = true
            });
            TotalEmotionsSeriesCollection.Add(new PieSeries
            {
                Title = "neutral",
                Values = new ChartValues<ObservableValue> { new ObservableValue(Convert.ToDouble(neutralPercents)) },
                DataLabels = true
            });
            TotalEmotionsSeriesCollection.Add(new PieSeries
            {
                Title = "sad",
                Values = new ChartValues<ObservableValue> { new ObservableValue(Convert.ToDouble(sadPercents)) },
                DataLabels = true
            });
            TotalEmotionsSeriesCollection.Add(new PieSeries
            {
                Title = "surprise",
                Values = new ChartValues<ObservableValue> { new ObservableValue(Convert.ToDouble(surprisePercents)) },
                DataLabels = true
            });
            TotalEmotionsSeriesCollection.Add(new PieSeries
            {
                Title = "angry",
                Values = new ChartValues<ObservableValue> { new ObservableValue(Convert.ToDouble(angryPercents)) },
                DataLabels = true
            });
            TotalEmotionsSeriesCollection.Add(new PieSeries
            {
                Title = "disgust",
                Values = new ChartValues<ObservableValue> { new ObservableValue(Convert.ToDouble(disgustPercents)) },
                DataLabels = true
            });
            TotalEmotionsSeriesCollection.Add(new PieSeries
            {
                Title = "fear",
                Values = new ChartValues<ObservableValue> { new ObservableValue(Convert.ToDouble(fearPercents)) },
                DataLabels = true
            });
        }

        public async void SendEmail() //object sender, RoutedEventArgs e
        {
            if (time == string.Empty)
            {
                return;
            }
            var myFrame = SnippingTool.Snip();
            /*            double Left = SystemParameters.VirtualScreenLeft;
                        double Top = SystemParameters.VirtualScreenTop;
                        double ScreenWidth = SystemParameters.VirtualScreenWidth;
                        double ScreenHeight = SystemParameters.VirtualScreenHeight;
            */
            if (myFrame == null || myFrame.Frame == null)
            {
                return;
            }
            System.Drawing.Image image = SnippingTool.FromRectangle(myFrame.Frame); //new System.Drawing.Rectangle(Convert.ToInt32(this.parentWindow.Left), Convert.ToInt32(this.parentWindow.Top), Convert.ToInt32(this.parentWindow.Width), Convert.ToInt32(this.parentWindow.Height))
            Byte[] imageBytes = ConversationPage.ImageToByte(image);
            var values = new Dictionary<string, string>
            {
                { "userName", userName },
                { "image", Convert.ToBase64String(imageBytes) },
                { "time", time }
            };
            string json = JsonConvert.SerializeObject(values, Formatting.Indented);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            string httpRegisterPost = "http://" + this.Ip + ":" + this.Port + "/statistics/email";
            try
            {
                var response = await client.PostAsync(httpRegisterPost, content);
                var responseString = await response.Content.ReadAsStringAsync();
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            break;
                        case HttpStatusCode.BadRequest:
                            if (responseString == "user not exist")
                            {
                                errorTxt.Text = "This user does not exist";
                            }
                            else if (responseString == "Error sending mail")
                            {
                                errorTxt.Text = "DB problem";
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
        private void Email_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendEmail();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.parentWindow.DragMove();
            }
        }

    }
}
