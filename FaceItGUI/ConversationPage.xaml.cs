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

        //private int i = 0;
        private bool stop = false, startCheck = false;
        //private string[] names = { "Gilad", "Israel", "Yossi" };
        private string currentName;
        private readonly string userName;
        private ConcurrentDictionary<string, string> namesMap;
        private static readonly HttpClient client = new HttpClient();


        /* private UdpClient Client;
         volatile Boolean Stop;
         private NetworkStream Stream;
         private List<NameInList> items;*/

        private int UdpPort;
        private string UdpIp;
        private int HttpPort;
        private string HttpIp;
        private const int maxMisses = 20;

        public ConversationPage(LoginWindow loginWindow, MainWindowPage main, string userName)
        {
            InitializeComponent();
            this.parentWindow = loginWindow;
            this.mainWinPage = main;
            // need to take the username from previous screen
            /*this.Client = null;
            this.Stop = false;*/
            this.UdpIp = ConfigurationManager.AppSettings["UdpIp"];
            this.UdpPort = Convert.ToInt32(ConfigurationManager.AppSettings["UdpPort"]);
            this.HttpIp = ConfigurationManager.AppSettings["HttpIp"];
            this.HttpPort = Convert.ToInt32(ConfigurationManager.AppSettings["HttpPort"]);
            this.namesMap = new ConcurrentDictionary<string, string>();
            this.userName = userName;
            allBehaviors = new ConcurrentDictionary<string, Behaviors>();
            missesRecognition = new ConcurrentDictionary<string, int>();
        }


        /*    public void Connect(string ip, int port)
            {
                try
                {
                    Stop = false;
                    this.Client = new UdpClient();
                    Client.Connect(ip, port);
                   // this.Stream = this.Client.GetStream();
                }
                catch
                {

                }
            }*/
        /*   public static UdpUser ConnectTo(string hostname, int port)
           {
               var connection = new UdpUser();
               connection.Client.Connect(hostname, port);
               return connection;
           }*/

        /*  public void Send(string message)
          {
              UdpClient udpClient = new UdpClient();
              udpClient.Connect(Ip, Port);
              Byte[] senddata = Encoding.ASCII.GetBytes("Hello World");
              Client.Send(senddata, senddata.Length);
              var datagram = Encoding.ASCII.GetBytes(message);
              Client.Send(datagram, datagram.Length);
              udpClient.Close();
          }
  */
        /*  public void Disconnect()
          {
              Stop = true;
              if (this.Stream != null)
              {
                  this.Stream.Close();
                  this.Stream = null;
              }
              if (this.Client != null)
              {
                  this.Client.Dispose();
                  this.Client.Close();
              }
          }*/


        public async Task<int> AddToNamesList(string name)
        {
            try
            {
                //namesListBorder.get
                //namesList.ItemContainerStyle.
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
                    //var myBorder = new Border();
                    //listGrid.Children.Add(myBorder)
                }));

                //int index = await namesList.Items.Add(new NameInList() { Name = currentName, Feeling = "" });
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
            var button = (System.Windows.Controls.Button)sender;
            if (button.Name == "meButton")
            {
                currentName = userName;
            }
            else if (nameBox.Text == string.Empty)
            {
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    errorTxt.Text = "Write the name first!";
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
            nameBox.Text = string.Empty;
            // items.Add(new NameInList() { Name = currentName, Feeling = "" });
            if (namesMap.ContainsKey(currentName))
            {
                System.Diagnostics.Debug.WriteLine(currentName + " already exists");
                //errorTxt.Text = currentName + " already exists";
                await Dispatcher.BeginInvoke((Action)(() => errorTxt.Text = currentName + " already exists"));

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

            //namesMap.Add(currentName, currentName);
            //int index = namesList.Items.Add(new NameInList() { Name = currentName, Feeling = "" });
            int index = await AddToNamesList(currentName);
            if (index == -1)
            {
                await Dispatcher.BeginInvoke((Action)(() => errorTxt.Text = currentName + " already exists"));
                return;
            }
            int maxUdpDatagramSize = 65515;
            //string mainDir = MakeScreenShotDirectory();

            if (currentName == userName)
            {
                currentName = "Y" + currentName;
                isLoginUser = true;
                meButton.Visibility = Visibility.Hidden;
                snipMeTxt.Visibility = Visibility.Hidden;
                othersTxt.Visibility = Visibility.Visible;
                nameBox.Visibility = Visibility.Visible;
                othersButton.Visibility = Visibility.Visible;

            }
            else
            {
                currentName = "N" + currentName;
                checkMatchBtn.Visibility = Visibility.Visible;
            }

            new Thread(async delegate ()
            {
                int round = 0;
                string thisName = currentName;
                // a little depends that row 52 will be executed before other snippinig tool capture
                //string id = names[i];
                //i++;
                //var currentSnip = myFrame.Clone();
                //string currentDir = MakeSpecificDirectory(mainDir, currentName);
                while (!stop)
                {
                    // take picture every 0.5 second !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    Thread.Sleep(500);

                    try
                    {
                        await Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            errorTxt.Text = string.Empty;
                        }));
                        long localDate = DateTime.Now.Ticks;
                        int numOfEndLines = 2;
                        string localDateStr = localDate.ToString();
                        System.Drawing.Image image = SnippingTool.FromRectangle(myFrame.Frame);
                        //int j = 1;
                        while (ImageToByte(image).Length + thisName.Length + localDateStr.Length + numOfEndLines > maxUdpDatagramSize)
                        {
                            image = ReduceImageSize(0.9, image);
                        }
                        //System.Diagnostics.Debug.WriteLine("sending " + actualDirectory + fileName);
                        WriteToServer(image, thisName, localDateStr, index, round, isLoginUser);

                        round++;
                        // byte[] data = ReadFromServer();

                        /*MemoryStream ms = new MemoryStream(data);
                        System.Drawing.Image img = System.Drawing.Image.FromStream(ms);*/
                        //System.Diagnostics.Debug.WriteLine("got the image: " + actualDirectory + fileName);
                        //                      System.Diagnostics.Debug.WriteLine("width: " + image.Width + " Height: " +image.Height);

                        // image.Save(actualDirectory + fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        //System.Diagnostics.Debug.WriteLine(fileName + " sent");

                        /* if (bmp != null)
                         {
                             bmp.Save("myImage" + i + ".png");
                             // Do something with the bitmap
                             //...
                             i++;
                         }*/
                    }
                    catch (Exception excep)
                    {
                        await Dispatcher.BeginInvoke((Action)(() => errorTxt.Text = "Problem with connection to server"));

                        System.Diagnostics.Debug.WriteLine("problem: " + excep.Message);
                    }
                }


            }).Start();
            this.parentWindow.WindowState = WindowState.Minimized;
            this.parentWindow.Show();
            // string folderPath = Server.MapPath("~/ImagesFolder/");  //Create a Folder in your Root directory on your solution.
            // string fileName = "~/screenshots/IMageName.jpg";
            /*string imagePath = folderPath + fileName;

            string base64StringData = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAlgAAADmCAYAAAA..........."; // Your base 64 string data
            string cleandata = base64StringData.Replace("data:image/png;base64,", "");
            byte[] data = System.Convert.FromBase64String(cleandata);
            MemoryStream ms = new MemoryStream(data);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);*/



        }


        public async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (errorTxt.Text != string.Empty && errorTxt.Text != "Write the name first!")
            {
                BackToMainWindow();
                return;
            }
            string httpStopRequest = "http://" + this.HttpIp + ":" + this.HttpPort + "/stop";
            try
            {
                var response = await client.GetAsync(httpStopRequest);

                var responseString = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        BackToMainWindow();
                        break;
                    case HttpStatusCode.BadRequest:
                        await Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            errorTxt.Text = "Server has problem with\n the DB";
                        }));
                        break;
                    default:
                        await Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            errorTxt.Text = "Connection problem\nwith server";
                        }));
                        break;
                }
            }
            catch
            {
                await Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    errorTxt.Text = "Connection problem\nwith server";
                }));
            }



            /*logWin.Show();
            this.Close();*/

        }

        private void BackToMainWindow()
        {
            this.stop = true;
            this.parentWindow.Content = mainWinPage;
        }


        private string MakeScreenShotDirectory()
        {
            string path = Directory.GetCurrentDirectory();
            string screenshotsDirectory = path + "\\Screenshots";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(screenshotsDirectory);
            }
            return screenshotsDirectory;
        }

        private string MakeSpecificDirectory(string mainDir, string name)
        {
            var actualDirectory = mainDir + "\\" + name;
            if (!Directory.Exists(actualDirectory))
            {
                Directory.CreateDirectory(actualDirectory);
            }
            return actualDirectory;
        }


        public async void WriteToServer(System.Drawing.Image image, string name, string fileName, int index, int round, bool isLoginUser)
        {
            if (image != null)
            {
                try
                {
                    UdpClient udpClient = new UdpClient();
                    udpClient.Connect(UdpIp, UdpPort);
                    byte[] dataName = Encoding.ASCII.GetBytes(name + "\n" + fileName + "\n");
                    //bytes 0xFF, 0xD9 indicate end of image
                    byte[] dataImage = ImageToByte(image);
                    byte[] bytes = new byte[dataName.Length + dataImage.Length];
                    //byte[] bytes = new byte[80000000000];

                    Buffer.BlockCopy(dataName, 0, bytes, 0, dataName.Length);
                    Buffer.BlockCopy(dataImage, 0, bytes, dataName.Length, dataImage.Length);
                    //System.Diagnostics.Debug.WriteLine("data length write: " + bytes.Length);
                    //System.Diagnostics.Debug.WriteLine("size is: " + bytes.Length);
                    //udpClient.Send(dataImage, dataImage.Length);

                    udpClient.Send(bytes, bytes.Length);
                    var result = await udpClient.ReceiveAsync();
                    var message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length);
                    await Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        // remove 'Y'/'N'
                        name = name.Substring(1);
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
                                errorTxt.Text = "Couldn't recognize\n" + name + "'s " + "face";
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
                                //namesList.Items[index] = myName;
                            }));

                        }
                    }));

                    //(NameInList)(namesList.Items.GetItemAt(index)).Feeling = message;
                    //namesList.();
                    //NameInList myName = (NameInList)namesList.Items[index];
                    //myName.Feeling = message;



                    //listBox1.Items.Add(thisFile.ToString());
                    System.Diagnostics.Debug.WriteLine(message);

                    // udpClient.Close();
                    // this.Stream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception e)
                {
                    await Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        errorTxt.Text = "Connection problem\nwith server";
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
                //int sumMatch = 0;
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

                // average
                //int avgMatch = sumMatch / numOfPeopleToCheck;

                // if (Math.Abs(userVibe - avgMatch) <= diff)
                // match most of participants
                if (matchedPeople >= numOfPeopleToCheck / 2.0)
                {
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
            }
        }

        /* public byte[] ReadFromServer()
         {
             bool end = false;
             if (this.Stream == null)
             {
                 //this.Error = "First Server!!";
                 return null;
             }
             byte[] data = new byte[1000000];
             string wow = "";

             //StringBuilder response = new StringBuilder();
             int i = 0;
             while (!end)
             {
                 Stream.Read(data, 0, data.Length);
                 for (; i < data.Length; i++)
                 {
                     //System.Diagnostics.Debug.WriteLine("Before if: " + i);
                     //System.Diagnostics.Debug.WriteLine((char)data[i]);
                     if ((char)data[i] == '\n')
                     {
                         //System.Diagnostics.Debug.WriteLine("in if: " + i);

                         end = true;
                         break;
                     }
                 }

                 //System.Diagnostics.Debug.WriteLine("after all: " + i);

                 wow = Encoding.ASCII.GetString(data, 0, i);
                 //System.Diagnostics.Debug.WriteLine(wow);

                 // wow
                 // end = true;
             }
             // return response.ToString();
             //System.Diagnostics.Debug.WriteLine("data length read: " + data.Length);

             return data;

         }*/

        public static byte[] ImageToByte(System.Drawing.Image iImage)
        {
            MemoryStream mMemoryStream = new MemoryStream();
            iImage.Save(mMemoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            return mMemoryStream.ToArray();
        }
    }
}
