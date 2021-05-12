using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FaceItGUI
{
    /// <summary>
    /// Interaction logic for SnippingWindow.xaml
    /// </summary>
    public partial class SnippingWindow : Window
    {
        private int Port;
        private string Ip;

        private NetworkStream Stream;
        private TcpClient Client;



        public SnippingWindow()
        {
            InitializeComponent();
            this.Ip = ConfigurationManager.AppSettings["Ip"];
            this.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
        }


        public void btnLogin_Click_1(object sender, RoutedEventArgs e)
        {
            //take username and password
            string userName = txtUserName.Text;
            string password = txtPassword.Password;
            // check userName and password
            this.Hide();
            MainWindow mainWin = new MainWindow(userName);
            mainWin.Show();
            this.Show();
            //Stream.Write()
        }


        public void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWin = new RegisterWindow();
            registerWin.Show();
        }
        public void Connect(string ip, int port)
        {
            try
            {
                this.Client = new TcpClient();
                Client.Connect(ip, port);
                this.Stream = this.Client.GetStream();
            }
            catch
            {

            }
        }

        public void Disconnect()
        {
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
    }
}
