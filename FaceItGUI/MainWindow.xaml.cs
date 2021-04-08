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

namespace FaceItGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int i = 0;
        private bool stop = false;
        private string[] names = { "Gilad", "Israel", "Yossi" };

        private UdpClient Client;
        volatile Boolean Stop;
        private NetworkStream Stream;

        private int Port;
        private string Ip;
        public MainWindow()
        {
            InitializeComponent();
            this.Client = null;
            this.Stop = false;
            this.Ip = ConfigurationManager.AppSettings["Ip"];
            this.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            /*this.Client = new UdpClient();
            Client.Connect(Ip, Port);*/


            //Connect(Ip,port);
        }
        public void Connect(string ip, int port)
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
        }
     /*   public static UdpUser ConnectTo(string hostname, int port)
        {
            var connection = new UdpUser();
            connection.Client.Connect(hostname, port);
            return connection;
        }*/

        public void Send(string message)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(Ip, Port);
            Byte[] senddata = Encoding.ASCII.GetBytes("Hello World");
            Client.Send(senddata, senddata.Length);
            var datagram = Encoding.ASCII.GetBytes(message);
            Client.Send(datagram, datagram.Length);
            udpClient.Close();
        }

        public void Disconnect()
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
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            int maxUdpDatagramSize = 65515;
            string path = Directory.GetCurrentDirectory();
            string screenshotsDirectory = path + "\\Screenshots";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(screenshotsDirectory);
            }
            this.ShowActivated = false;
            this.Hide();
            Thread.Sleep(300);
            var myFrame = SnippingTool.Snip();

            new Thread(delegate ()
            {
                // a little depends that row 52 will be executed before other snippinig tool capture
                string id = names[i];
                i++;
                //var currentSnip = myFrame.Clone();
                var actualDirectory = screenshotsDirectory + "\\" + id;
                if (!Directory.Exists(actualDirectory))
                {
                    Directory.CreateDirectory(actualDirectory);
                }
                while (!stop)
                {
                    // take picture every 0.5 second !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    Thread.Sleep(1000);
                    try
                    {

                        long localDate = DateTime.Now.Ticks;

                        string fileName = "\\" + localDate + ".jpg";
                        Image image = SnippingTool.FromRectangle(myFrame.Frame);
                        //int j = 1;
                        while (ImageToByte(image).Length + fileName.Length > maxUdpDatagramSize)
                        {
                            // maybe calculate image only at the end!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            image = ReduceImageSize(0.9, image);
                            System.Diagnostics.Debug.WriteLine("Height: " + image.Height + " Width: " + image.Width);
                        }
                        //System.Diagnostics.Debug.WriteLine("sending " + actualDirectory + fileName);
                        WriteToServer(image, id, fileName);
                        // byte[] data = ReadFromServer();

                        /*   MemoryStream ms = new MemoryStream(data);
                           System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                           //System.Diagnostics.Debug.WriteLine("got the image: " + actualDirectory + fileName);*/
                        //                      System.Diagnostics.Debug.WriteLine("width: " + image.Width + " Height: " +image.Height);

                        image.Save(actualDirectory + fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        //System.Diagnostics.Debug.WriteLine(fileName + " sent");

                        /*if (bmp != null)
                        {
                            bmp.Save("myImage" + i + ".png");
                            // Do something with the bitmap
                            //...
                            i++;
                        }*/
                    }
                    catch (Exception excep)
                    {
                        //System.Diagnostics.Debug.WriteLine("problem: " + excep.Message);
                    }
                }


            }).Start();
            WindowState = WindowState.Minimized;
            this.Show();
            // string folderPath = Server.MapPath("~/ImagesFolder/");  //Create a Folder in your Root directory on your solution.
            // string fileName = "~/screenshots/IMageName.jpg";
            /*string imagePath = folderPath + fileName;

            string base64StringData = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAlgAAADmCAYAAAA..........."; // Your base 64 string data
            string cleandata = base64StringData.Replace("data:image/png;base64,", "");
            byte[] data = System.Convert.FromBase64String(cleandata);
            MemoryStream ms = new MemoryStream(data);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);*/



        }

        public async void WriteToServer(Image image, string name, string fileName)
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
                System.Diagnostics.Debug.WriteLine("size is: " + bytes.Length);

                udpClient.Send(bytes, bytes.Length);
                var result = await udpClient.ReceiveAsync();
                var message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length);
                System.Diagnostics.Debug.WriteLine(message);

               // udpClient.Close();
                // this.Stream.Write(bytes, 0, bytes.Length);

            }
        }
        private Image ReduceImageSize(double scaleFactor, Image image)
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

        public byte[] ReadFromServer()
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

        }

        public static byte[] ImageToByte(Image iImage)
        {
            MemoryStream mMemoryStream = new MemoryStream();
            iImage.Save(mMemoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            return mMemoryStream.ToArray();
        }
    }
}