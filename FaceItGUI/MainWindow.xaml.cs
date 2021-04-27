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
        private string currentName;
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

        
    }
}