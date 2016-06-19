﻿using System;
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
using System.Diagnostics;

namespace FormBasedTCPListenMaster
{
    public partial class Form1 : Form
    {
        public static masterTCPClient client;
        public Form1()
        {
            InitializeComponent();
            client = new masterTCPClient();
        }

       private static Task<string> runClientWriteAsync(byte[] msg, IPAddress addr)
        {
            CancellationToken ct;
           
            Task<string> tsResponse = client.SendRequest(msg, addr, ct);
           
            return (tsResponse);
        }

       private static Task<string> runClientReadAsync(byte[] msg, IPAddress addr)
       {
           CancellationToken ct;

           Task<string> tsResponse = client.ReadRequest(msg, addr);

           return (tsResponse);
       }
        private void btnStartMaster_Click(object sender, EventArgs e)
        {
            
            string dataToSend;

           /* for (int i = 0; i < 10; i++)
            {
                dataToSend = "Packet:: " + Convert.ToString(i);

                string reply = await runClientAsync(dataToSend);
                textBox1.Text += reply;
            }*/
            client.setLocalAddr(textBoxAddresses);
            textBox1.Text += "Master Started on addr: " + client.localAddr + Environment.NewLine;
            client.listenForOutstations(textBox1);
             
        } 


        private async void btnWriteData_Click(object sender, EventArgs e)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start(); 
            //Send a write packet to the Outstation
            IPAddress addr = IPAddress.Parse(txtBoxWriteIPAddr.Text);
            List<byte> dnpPkt = new List<byte>();
            buildPkt(ref dnpPkt);
            byte[] msgBytes = dnpPkt.ToArray();

            //string msg = BitConverter.ToString(msgBytes);
            //Console.WriteLine(msg);  
             
            string response = await runClientWriteAsync(msgBytes, addr);
             
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            textBox1.Text += response + Environment.NewLine;
            stopWatch.Reset();
            
        }

        public void buildPkt(ref List<byte> dnpPkt)
        {
            APDU myApdu = new APDU();
            TPDU myTpdu = new TPDU();
            
            //params should be in the following order
            //Confirm, Unsolicited, function, group, variation, prefixQualifier, [range] OR [start index, stop index]
            //myApdu.buildAPDU(ref dnpPkt, 0x00, 0x00, 0x01, 0x0A, 0x01, 0x00, 0x00, 0x02, 0x01, 0x01, 0x01);
            myApdu.buildAPDU(ref dnpPkt, 0x00, 0x00, 0x01, 0x01, 0x01, 0x00, 0x00, 0x02);

            myTpdu.buildTPDU(ref dnpPkt);
            
            DPDU myDpdu = new DPDU();
            myDpdu.buildDPDU(ref dnpPkt, 0xC4, 1, 65519); //dst=1, src=65519
           

        }


        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnReadData_Click(object sender, EventArgs e)
        {

            client.listenForOutstations(textBox1);

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }
    }
}
