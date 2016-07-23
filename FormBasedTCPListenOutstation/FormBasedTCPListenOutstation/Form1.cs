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
using System.Diagnostics;



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
                service.setLocalAddr(textBoxAddresses); //set our local addr for later use 
                service.Run(textBox1, radioButton1, radioButton2, radioButton3);
                textBox1.Text += "running GUI" + Environment.NewLine;
                applyLoad();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }
        }

        public static void CPUKill(object cpuUsage)
        {
            Parallel.For(0, 1, new Action<int>((int i) =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                while (true)
                {
                    if (watch.ElapsedMilliseconds > (int)cpuUsage)
                    {
                        Thread.Sleep(100 - (int)cpuUsage);
                        watch.Reset();
                        watch.Start();
                    }
                }
            }));

        }

        private void applyLoad()
        {
            int cpuUsage = Convert.ToInt32(txtBoxCPULoad.Text);
            int time = 100;
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(CPUKill));
                t.Start(cpuUsage);
                threads.Add(t);
            }
            Thread.Sleep(time);
            foreach (var t in threads)
            {
                t.Abort();
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
                
                await service.buildISP(textBoxIpAddrDS.Text, txtBxSplitClientIP.Text, textBoxHWAddress.Text);
            }
            catch
            {
                Console.WriteLine("service.sendISP exception!");
            }
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        
    }
}
