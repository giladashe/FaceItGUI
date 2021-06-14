using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Web;
using System.Drawing;
using System.Net.Sockets;
using System.Configuration;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Controls;
using System.Reflection;
using System.Net.Http;
using System.Net;
using System.Collections.Concurrent;
using System.Timers;

namespace FaceItGUI
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class ConversationPage : Page
    {
        private LoginWindow parentWindow;
        private MainWindowPage mainWinPage;
        // thread safe dictionary for behaviors to a specific person
        private ConcurrentDictionary<string, Behaviors> allBehaviors;

        // thread safe dictionary for behaviors to a specific person
        private ConcurrentDictionary<string, int> missesRecognition;

        private bool stop = false, startCheck = false;
        private readonly string userName;
        private ConcurrentDictionary<string, string> namesMap;
        private static readonly HttpClient client = new HttpClient();
        private bool startedConversation = false;
        private DateTime lastTime;
        private System.Timers.Timer blankSpaceTimer;
        private int allChecks = 0;
        private int allMatches = 0;


        private int UdpPort;
        private string UdpIp;
        private int HttpPort;
        private string HttpIp;
        private const int maxMisses = 20;

        private static Mutex mut = new Mutex();

        public ConversationPage(LoginWindow loginWindow, MainWindowPage main, string userName)
        {
            InitializeComponent();
            this.parentWindow = loginWindow;
            this.mainWinPage = main;
            this.UdpIp = ConfigurationManager.AppSettings["UdpIp"];
            this.UdpPort = Convert.ToInt32(ConfigurationManager.AppSettings["UdpPort"]);
            this.HttpIp = ConfigurationManager.AppSettings["HttpIp"];
            this.HttpPort = Convert.ToInt32(ConfigurationManager.AppSettings["HttpPort"]);
            this.namesMap = new ConcurrentDictionary<string, string>();
            this.userName = userName;
            this.lastTime = DateTime.Now;
            this.blankSpaceTimer = null;
            allBehaviors = new ConcurrentDictionary<string, Behaviors>();
            missesRecognition = new ConcurrentDictionary<string, int>();
        }

        public async Task<int> AddToNamesList(string name)
        {
            try
            {
                namesMap.TryAdd(name, name);
                Behaviors behaviors = new Behaviors
                {
                    CheckMatch = true
                };
                allBehaviors.TryAdd(name, behaviors);
                missesRecognition.TryAdd(name, 0);
                int index = 0;
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    index = namesList.Items.Add(new NameInList() { Name = name, Feeling = "" });
                }));

                return index;
            }
            catch
            {
                // name already exists
                return -1;
            }

        }

        private async void MyButton_Click(object sender, RoutedEventArgs e)
        {
            string currentName = string.Empty;
            var button = (System.Windows.Controls.Button)sender;
            if (button.Name == "meButton")
            {
                currentName = "You";
            }
            else if (nameBox.Text == string.Empty)
            {
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    string message = "Write the name first!";
                    if (Math.Abs((DateTime.Now - this.lastTime).TotalSeconds) > 5 && errorTxt.Text != message)
                    {
                        errorTxt.Text = message;
                    }
                }));
                return;
            }
            else
            {
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    currentName = nameBox.Text;
                }));
            }
            if (namesMap.ContainsKey(currentName))
            {
                System.Diagnostics.Debug.WriteLine(currentName + " already exists");
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    string message = $"{currentName} already exists";
                    if (Math.Abs((DateTime.Now - this.lastTime).TotalSeconds) > 5 && errorTxt.Text != message)
                    {
                        errorTxt.Text = message;
                    }
                }));
                return;
            }
            this.parentWindow.ShowActivated = false;
            this.parentWindow.Hide();
            Thread.Sleep(300);
            var myFrame = SnippingTool.Snip();
            bool isLoginUser = false;
            if (myFrame == null)
            {
                this.parentWindow.Show();
                return;
            }
            if (!startedConversation)
            {
                this.blankSpaceTimer = SetTimer();
                this.blankSpaceTimer.Start();
            }
            startedConversation = true;
            int index = await AddToNamesList(currentName);
            if (index == -1)
            {
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    string message = $"{currentName} already exists";
                    if (Math.Abs((DateTime.Now - this.lastTime).TotalSeconds) > 5 && errorTxt.Text != message)
                    {
                        errorTxt.Text = message;
                    }
                }));
                return;
            }
            int maxUdpDatagramSize = 65515;

            if (currentName == "You")
            {
                isLoginUser = true;
                meButton.Visibility = Visibility.Hidden;
                snipMeTxt.Visibility = Visibility.Hidden;
                othersTxt.Visibility = Visibility.Visible;
                nameBox.Visibility = Visibility.Visible;
                othersButton.Visibility = Visibility.Visible;

            }
            else
            {
                checkMatchBtn.Visibility = Visibility.Visible;
            }

            new Thread(async delegate ()
            {
                int round = 0;
                while (!stop)
                {
                    // takes picture every 0.5 second 
                    Thread.Sleep(500);
                    try
                    {
                        //long localDate = DateTime.Now.Ticks;
                        string localDateStr = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                        int numOfEndLines = 2;
                        //string localDateStr = localDate.ToString();
                        System.Drawing.Image image = SnippingTool.FromRectangle(myFrame.Frame);
                        while (ImageToByte(image).Length + currentName.Length + localDateStr.Length + numOfEndLines > maxUdpDatagramSize)
                        {
                            image = ReduceImageSize(0.9, image);
                        }
                        WriteToServer(image, currentName, localDateStr, index, round, isLoginUser);

                        round++;
                    }
                    catch (Exception excep)
                    {
                        await Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            string message = "Connection problem\nwith server";
                            if (Math.Abs((DateTime.Now - this.lastTime).TotalSeconds) > 5 && errorTxt.Text != message)
                            {
                                errorTxt.Text = message;
                            }

                        }));
                        System.Diagnostics.Debug.WriteLine("problem: " + excep.Message);
                    }
                }


            }).Start();
            this.parentWindow.WindowState = WindowState.Minimized;
            this.parentWindow.Show();

        }


        private System.Timers.Timer SetTimer()
        {
            // Create a timer with a two second interval.
            System.Timers.Timer aTimer = new System.Timers.Timer(7000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            return aTimer;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                this.lastTime = DateTime.Now;
                errorTxt.Text = string.Empty;
            }));
        }

        public async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!startedConversation)
            {
                BackToMainWindow();
                return;
            }
            mut.WaitOne();
            var values = new Dictionary<string, string>
            {
                { "checks", allChecks.ToString() },
                { "matches", allMatches.ToString() }
            };
            mut.ReleaseMutex();
            var content = new FormUrlEncodedContent(values);
            string httpStopRequest = "https://" + this.HttpIp + ":" + this.HttpPort + "/stop";
            try
            {
                var response = await client.PostAsync(httpStopRequest, content);
                var responseString = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        this.blankSpaceTimer.Stop();
                        BackToMainWindow();
                        break;
                    case HttpStatusCode.BadRequest:
                        await Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            string message = "Server has problem with\n the DB";
                            if (Math.Abs((DateTime.Now - this.lastTime).TotalSeconds) > 5 && errorTxt.Text != message)
                            {
                                errorTxt.Text = message;
                            }
                        }));
                        break;
                    default:
                        await Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            string message = "Connection problem\nwith server";
                            if (Math.Abs((DateTime.Now - this.lastTime).TotalSeconds) > 5 && errorTxt.Text != message)
                            {
                                errorTxt.Text = message;
                            }
                           
                        }));
                        break;
                }
            }
            catch
            {
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    string message = "Connection problem\nwith server";
                    if (Math.Abs((DateTime.Now - this.lastTime).TotalSeconds) > 5 && errorTxt.Text != message)
                    {
                        errorTxt.Text = message;
                    }

                }));
            }


        }

        private void BackToMainWindow()
        {
            this.stop = true;
            this.parentWindow.Content = mainWinPage;
        }



        public async void WriteToServer(System.Drawing.Image image, string name, string dateTime, int index, int round, bool isLoginUser)
        {
            if (image != null)
            {
                try
                {
                    UdpClient udpClient = new UdpClient();
                    udpClient.Connect(UdpIp, UdpPort);
                    // if the login user sends it sends only "Y", else send name
                    string nameToSend = isLoginUser ? "Y" : name;
                    byte[] dataName = Encoding.ASCII.GetBytes(nameToSend + "\n" + dateTime + "\n");
                    //bytes 0xFF, 0xD9 indicate end of image
                    byte[] dataImage = ImageToByte(image);
                    byte[] bytes = new byte[dataName.Length + dataImage.Length];

                    Buffer.BlockCopy(dataName, 0, bytes, 0, dataName.Length);
                    Buffer.BlockCopy(dataImage, 0, bytes, dataName.Length, dataImage.Length);

                    udpClient.Send(bytes, bytes.Length);
                    var result = await udpClient.ReceiveAsync();
                    var message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length);
                    await Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        bool goodVibe = (message == "happy" || message == "surprise");
                        bool neutral = (message == "neutral");
                        bool foundFace = false;

                        PropertyInfo[] properties = typeof(Behaviors).GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.Name == "Match" || property.Name == "CheckMatch")
                            {
                                continue;
                            }

                            if (property.Name.ToLower() == message)
                            {
                                int value = (int)property.GetValue(allBehaviors[name]);
                                property.SetValue(allBehaviors[name], value + 1);
                                allBehaviors[name].CheckMatch = true;
                                missesRecognition[name] = 0;
                                foundFace = true;
                                break;
                            }

                        }

                        if (!foundFace)
                        {
                            missesRecognition[name]++;
                        }

                        if (missesRecognition[name] >= maxMisses)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate ()
                            {
                                string message = $"Couldn't recognize\n {name}'s face";
                                if (Math.Abs((DateTime.Now - this.lastTime).TotalSeconds) > 5 && errorTxt.Text != message)
                                {
                                    errorTxt.Text = message;
                                }
                            }));
                            return;
                        }

                        if (startCheck)
                        {
                            // check matching to others every 15 rounds
                            bool needToCheck = (isLoginUser && round % 15 == 0);
                            AddAndCheckMatch(name, neutral, goodVibe, isLoginUser, needToCheck);
                        }

                        // changes the feeling on screen according to most feelings he had the last 15 rounds
                        if (round % 10 == 0 || round == 1)
                        {
                            int maxPropertyValue = -1;
                            //PropertyInfo maxProperty = (properties[0].Name != "Match") ? properties[0] : properties[1];
                            PropertyInfo maxProperty = null;
                            foreach (PropertyInfo property in properties)
                            {
                                if (property.Name == "Match" || property.Name == "CheckMatch")
                                {
                                    continue;
                                }
                                int propertyValue = (int)property.GetValue(allBehaviors[name]);

                                if (maxProperty == null || propertyValue > maxPropertyValue)
                                {
                                    maxPropertyValue = propertyValue;
                                    maxProperty = property;
                                    property.SetValue(allBehaviors[name], 0);
                                }

                            }
                            Dispatcher.BeginInvoke(new Action(delegate ()
                            {
                                if (maxProperty == null)
                                    return;
                                ((NameInList)namesList.Items[index]).Feeling = maxProperty.Name.ToLower();
                                NameInList myName = (NameInList)namesList.Items[index];
                                System.Diagnostics.Debug.WriteLine(myName.Name + ": " + myName.Feeling);
                            }));

                        }
                    }));
                    System.Diagnostics.Debug.WriteLine(message);
                }
                catch (Exception e)
                {
                    await Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        string message = "Connection problem\nwith server";
                        if (Math.Abs((DateTime.Now - this.lastTime).TotalSeconds) > 5 && errorTxt.Text != message)
                        {
                            errorTxt.Text = message;
                        }
                        System.Diagnostics.Debug.WriteLine(e.Message);

                    }));
                }
            }
        }

        private System.Drawing.Image ReduceImageSize(double scaleFactor, System.Drawing.Image image)
        {
            var newWidth = (int)(image.Width * scaleFactor);
            var newHeight = (int)(image.Height * scaleFactor);
            var thumbnailImg = new Bitmap(newWidth, newHeight);
            var thumbGraph = Graphics.FromImage(thumbnailImg);
            thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
            thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
            thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
            thumbGraph.DrawImage(image, imageRectangle);
            return thumbnailImg;
        }


        public void CheckMatchOnClick(object sender, RoutedEventArgs e)
        {
            othersButton.Visibility = Visibility.Hidden;
            othersTxt.Visibility = Visibility.Hidden;
            nameBox.Visibility = Visibility.Hidden;
            checkMatchBtn.Visibility = Visibility.Hidden;
            othersButton.Visibility = Visibility.Hidden;
            startCheck = true;
        }

        public void AddAndCheckMatch(string name, bool neutral, bool goodVibe, bool isLoginUser, bool needToCheck)
        {
            if (!neutral)
            {
                if (goodVibe)
                {
                    allBehaviors[name].Match++;
                }
                else
                {
                    allBehaviors[name].Match--;
                }
            }
            if (isLoginUser && needToCheck)
            {
                Behaviors userBehaviors = allBehaviors[name];
                int userVibe = userBehaviors.Match;
                int diff = 5;
                double numOfPeopleToCheck = 0.0;
                double matchedPeople = 0.0;
                foreach (var behavior in allBehaviors)
                {
                    Behaviors behaviorVal = behavior.Value;
                    if (behavior.Key != name || behaviorVal.CheckMatch == false)
                    {
                        if (Math.Abs(userVibe - behaviorVal.Match) <= diff)
                        {
                            matchedPeople++;
                        }
                        numOfPeopleToCheck++;
                    }
                    behavior.Value.Match = 0;
                }
                
                if (numOfPeopleToCheck == 0.0)
                    return;
                mut.WaitOne();
                allChecks++;
                // average
                //int avgMatch = sumMatch / numOfPeopleToCheck;

                // if (Math.Abs(userVibe - avgMatch) <= diff)
                // match most of participants
                if (matchedPeople >= numOfPeopleToCheck / 2.0)
                {
                    allMatches++;
                    //good match
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        matchText.Text = "You match your\n interlocutors :)";
                    }));
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        //bad match
                        matchText.Text = "You need to adjust your\n behaviors to the\n conversation!";
                    }));
                }
                mut.ReleaseMutex();
            }
        }

        public static byte[] ImageToByte(System.Drawing.Image iImage)
        {
            MemoryStream mMemoryStream = new MemoryStream();
            iImage.Save(mMemoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            return mMemoryStream.ToArray();
        }
    }
}
