using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Configuration;

namespace FaceItGUI
{
    class Connector
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

        public Connector()
        {
            this.Stop = false;
            this.Ip = ConfigurationManager.AppSettings["Ip"];
            this.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
        }

        public void ProcessImages()
    }
}
