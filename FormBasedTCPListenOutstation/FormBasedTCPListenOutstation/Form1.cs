using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;



namespace FormBasedTCPListenOutstation
{
  

    public partial class Form1 : Form
    {
        TCPListen service;
         

        public Form1()
        {
            
            service = new TCPListen();  

           

            InitializeComponent();
        } 

        private  void button1_Click(object sender, EventArgs e)
        {
            try
            {
                service.Run(textBox1, radioButton1, radioButton2, radioButton3);
                Console.Write("running GUI" + Environment.NewLine);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }
        }

        private async void buttonSeekDataServer_Click(object sender, EventArgs e)
        {
            //We want to send an ISP Data Server Request to a nearby DNP station.  
            CancellationToken ct;
            try
            {
                await service.writeToClient("ABCD", textBox1, ct);
            }
            catch (Exception)
            {
                Console.WriteLine("service.writeToClient exception!");
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
               
                await service.buildISP(textBoxIpAddrDS.Text);
            }
            catch
            {
                Console.WriteLine("service.sendISP exception!");
            }
            
        }
    }
}
