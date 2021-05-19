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

namespace FaceItGUI
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class ConversationPage : Page
    {
        private LoginWindow parentWindow;
        private MainWindowPage mainWinPage;
        private Dictionary<string, Behaviors> allBehaviors;

        public ConversationPage(LoginWindow loginWindow, MainWindowPage main, string userName)
        {
            InitializeComponent();
            this.parentWindow = loginWindow;
            this.mainWinPage = main;
            // need to take the username from previous screen
            /*this.Client = null;
            this.Stop = false;*/
            this.Ip = ConfigurationManager.AppSettings["Ip"];
            this.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            this.namesMap = new Dictionary<string, string>();
            this.userName = userName;
            allBehaviors = new Dictionary<string, Behaviors>();
            // items = new List<NameInList>();
            //items.Add(new NameInList() { Name = "Yossi", Feeling = "" });
            //namesList.ItemsSource = items;
            /*this.Client = new UdpClient();
            Client.Connect(Ip, Port);*/


            //Connect(Ip,port);
        }
        //private int i = 0;
        private bool stop = false;
        //private string[] names = { "Gilad", "Israel", "Yossi" };
        private string currentName;
        private readonly string userName;
        private Dictionary<string, string> namesMap;

        /* private UdpClient Client;
         volatile Boolean Stop;
         private NetworkStream Stream;
         private List<NameInList> items;*/

        private int Port;
        private string Ip;

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
            namesMap.Add(currentName, currentName);
            allBehaviors.Add(name, new Behaviors());
            int index = 0;
            await Dispatcher.BeginInvoke(new Action(delegate ()
            {
                index = namesList.Items.Add(new NameInList() { Name = currentName, Feeling = "" });
            }));

            //int index = await namesList.Items.Add(new NameInList() { Name = currentName, Feeling = "" });
            return index;
        }

        private async void MyButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            currentName = (button.Name == "meButton") ? userName : nameBox.Text;
            // items.Add(new NameInList() { Name = currentName, Feeling = "" });
            if (namesMap.ContainsKey(currentName))
            {
                System.Diagnostics.Debug.WriteLine(currentName + " already exists");
                return;
            }
            //namesMap.Add(currentName, currentName);
            //int index = namesList.Items.Add(new NameInList() { Name = currentName, Feeling = "" });
            int index = await AddToNamesList(currentName);

            int maxUdpDatagramSize = 65515;
            //string mainDir = MakeScreenShotDirectory();
            this.parentWindow.ShowActivated = false;
            this.parentWindow.Hide();
            Thread.Sleep(300);
            var myFrame = SnippingTool.Snip();
            if (myFrame == null)
            {
                return;
            }
            if (currentName == userName)
            {
                currentName = "Y" + currentName;
            }
            else
            {
                currentName = "N" + currentName;
            }

            new Thread(delegate ()
            {
                int round = 0;
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

                        long localDate = DateTime.Now.Ticks;
                        int numOfEndLines = 2;
                        string localDateStr = localDate.ToString();
                        System.Drawing.Image image = SnippingTool.FromRectangle(myFrame.Frame);
                        //int j = 1;
                        while (ImageToByte(image).Length + currentName.Length + localDateStr.Length + numOfEndLines > maxUdpDatagramSize)
                        {
                            // maybe calculate image only at the end!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            image = ReduceImageSize(0.9, image);
                            // System.Diagnostics.Debug.WriteLine("Height: " + image.Height + " Width: " + image.Width);
                        }
                        //System.Diagnostics.Debug.WriteLine("sending " + actualDirectory + fileName);
                        WriteToServer(image, currentName, localDateStr, index, round);
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


        public void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.stop = true;
            // todo send to Israel!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            this.parentWindow.Content = mainWinPage;


            /*logWin.Show();
            this.Close();*/

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


        public async void WriteToServer(System.Drawing.Image image, string name, string fileName, int index, int round)
        {
            if (image != null)
            {

                UdpClient udpClient = new UdpClient();
                udpClient.Connect(Ip, Port);
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

                    switch (message)
                    {
                        case "sad":
                            allBehaviors[name].Sad++;
                            break;
                        case "happy":
                            allBehaviors[name].Happy++;
                            break;
                        case "surprise":
                            allBehaviors[name].Surprise++;
                            break;
                        case "angry":
                            allBehaviors[name].Angry++;
                            break;
                        case "disgust":
                            allBehaviors[name].Disgust++;
                            break;
                        case "fear":
                            allBehaviors[name].Fear++;
                            break;
                        default:
                            break;
                    }

                    if (round!=0 && round % 20 == 0)
                    {
                        PropertyInfo[] properties = typeof(Behaviors).GetProperties();
                        int maxPropertyValue = -1;
                        PropertyInfo maxProperty = properties[0];
                        foreach (PropertyInfo property in properties)
                        {
                            int propertyValue = (int)property.GetValue(allBehaviors[name]);
                            if (propertyValue > maxPropertyValue)
                            {
                                maxPropertyValue = propertyValue;
                                maxProperty = property;
                                property.SetValue(allBehaviors[name], 0);
                            }

                        }

                        ((NameInList)namesList.Items[index]).Feeling = maxProperty.Name.ToLower();
                        NameInList myName = (NameInList)namesList.Items[index];
                        System.Diagnostics.Debug.WriteLine(myName.Name + ": " + myName.Feeling);
                        //namesList.Items[index] = myName;

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
