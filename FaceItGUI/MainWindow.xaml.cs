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

        private TcpClient Client;
        volatile Boolean Stop;
        private NetworkStream Stream;

        private string Port;
        private string Ip;
        public MainWindow()
        {
            InitializeComponent();
            this.Client = null;
            this.Stop = false;
            this.Ip = ConfigurationManager.AppSettings["Ip"];
            this.Port = ConfigurationManager.AppSettings["Port"];
            Connect(Ip, Convert.ToInt32(Port));
        }
        public void Connect(string ip, int port)
        {
            try
            {
                Stop = false;
                this.Client = new TcpClient
                {
                    ReceiveTimeout = 10000
                };
                Client.Connect(ip, port);
                this.Stream = this.Client.GetStream();
            }
            catch
            {

            }
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
                var currentSnip = myFrame.Clone();
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
                        Image image = SnippingTool.FromRectangle(currentSnip.Frame);
                        /* Image image = new Bitmap(currentSnip.Frame.Width, currentSnip.Frame.Height);
                         using (Graphics gr = Graphics.FromImage(image))
                         {
                             gr.DrawImage(this.BackgroundImage, new Rectangle(0, 0, FirstImage.Width, FirstImage.Height),
                                 rcSelect, GraphicsUnit.Pixel);
                         }*/
                        System.Diagnostics.Debug.WriteLine("sending " + actualDirectory + fileName);
                        WriteToServer(image, id, fileName);
                        byte[] data = ReadFromServer();
                        
                        MemoryStream ms = new MemoryStream(data);
                        System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                        System.Diagnostics.Debug.WriteLine("got the image: " + actualDirectory + fileName);

                        img.Save(actualDirectory + fileName, System.Drawing.Imaging.ImageFormat.Jpeg);

                        System.Diagnostics.Debug.WriteLine(fileName + " sent");

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
                        System.Diagnostics.Debug.WriteLine("problem: " + excep.Message);
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

        public void WriteToServer(Image image, string name, string fileName)
        {
            name += '\n';
            if (image != null && this.Stream != null)
            {
                
                byte[] dataName = Encoding.ASCII.GetBytes(name + "\n" + fileName + "\n");
                //bytes 0xFF, 0xD9 indicate end of image
                byte[] dataImage = ImageToByte(image);
                byte[] bytes = new byte[dataName.Length + dataImage.Length];
                Buffer.BlockCopy(dataName, 0, bytes, 0, dataName.Length);
                Buffer.BlockCopy(dataImage, 0, bytes, dataName.Length, dataImage.Length);
                System.Diagnostics.Debug.WriteLine("data length write: " + bytes.Length);

                this.Stream.Write(bytes, 0, bytes.Length);

            }
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
                    System.Diagnostics.Debug.WriteLine("Before if: " + i);
                    System.Diagnostics.Debug.WriteLine((char)data[i]);
                    if ((char)data[i] == '\n')
                    {
                        System.Diagnostics.Debug.WriteLine("in if: " + i);

                        end = true;
                        break;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("after all: " + i);

                wow = Encoding.ASCII.GetString(data, 0, i);
                System.Diagnostics.Debug.WriteLine(wow);

                // wow
                // end = true;
            }
            // return response.ToString();
            System.Diagnostics.Debug.WriteLine("data length read: " + data.Length);

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