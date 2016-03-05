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

        private  async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text += "Outstation Start" + Environment.NewLine;
                service.setLocalAddr(); //set our local addr for later use
                
                service.Run(textBox1, radioButton1, radioButton2, radioButton3);
                textBox1.Text += "running GUI" + Environment.NewLine;

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
                
                await service.buildISP(textBoxIpAddrDS.Text, txtBxSplitClientIP.Text);
            }
            catch
            {
                Console.WriteLine("service.sendISP exception!");
            }
            
        }

        
    }
}
