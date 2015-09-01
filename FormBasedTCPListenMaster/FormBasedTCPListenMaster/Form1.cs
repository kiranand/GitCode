using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace FormBasedTCPListenMaster
{
    public partial class Form1 : Form
    {
        public static TCPClient client;
        public Form1()
        {
            InitializeComponent();
            client = new TCPClient();
        }

       private static Task<string> runClientAsync(string textToSend)
        {
           
            Task<string> tsResponse = client.SendRequest(textToSend);
           
            return (tsResponse);
        }

        private async void btnStartMaster_Click(object sender, EventArgs e)
        {
            
            string dataToSend;

            for (int i = 0; i < 10; i++)
            {
                dataToSend = "Packet:: " + Convert.ToString(i);

                string reply = await runClientAsync(dataToSend);
                textBox1.Text += reply;
            }
        }
    }
}
