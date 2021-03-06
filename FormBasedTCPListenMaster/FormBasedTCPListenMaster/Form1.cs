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
using System.Collections.Concurrent;
using System.Text;

namespace FormBasedTCPListenMaster
{
    public partial class Form1 : Form
    {
        public static masterTCPClient client;

        public static byte[] dataToSend = new byte[3];
        public static byte[] dataFromOutStation = new byte[3];
        public static ConcurrentQueue<string> unitTestComplete = new ConcurrentQueue<string>();
        public static AutoResetEvent unitTestDone = new AutoResetEvent(false);  
        public static string pathString;
        
        
 


        public Form1()
        {
            string myFileName = String.Format("{0}__{1}", DateTime.Now.ToString("yyyyMMddhhnnss"), "dnpTest.txt");
             pathString = Path.Combine("C:\\Users\\Anand\\Documents\\DNPTestOut", myFileName);
            
            if (!File.Exists(pathString))
            {
                // Create a file to write to. 
                File.WriteAllText(pathString, ("Start Test"+ Environment.NewLine));
            }
            InitializeComponent();
            client = new masterTCPClient(); 
        }

       private static Task<string> runClientWriteAsync(byte[] msg, IPAddress addr)
        {
            CancellationToken ct;
           
            Task<string> tsResponse = client.SendRequest(msg, addr, ct);
            Console.WriteLine("Sending DNP3 pkt" + Environment.NewLine);
            return (tsResponse);
        }

       private static Task<string> runClientReadAsync(byte[] msg, IPAddress addr)
       {
           CancellationToken ct;

           Task<string> tsResponse = client.ReadRequest(msg, addr);

           return (tsResponse);
       }
        private  async void btnStartMaster_Click(object sender, EventArgs e)
        {    
            string dataToSend; 
           /* for (int i = 0; i < 10; i++)
            {
                dataToSend = "Packet:: " + Convert.ToString(i);

                string reply = await runClientAsync(dataToSend);
                textBox1.Text += reply;
            }*/

            masterTCPClient.dnp3Pkt.Clear();
            client.setLocalAddr(textBoxAddresses);
            textBox1.Text += "Master Started on addr: " + client.localAddr + Environment.NewLine;
            client.listenForOutstations(textBox1);
            Thread checkResponse = new Thread(new ThreadStart(checkDNPReqResp));
            checkResponse.Start();
             
        }

     
        private async void btnWriteData_Click(object sender, EventArgs e)
        {
            
            //Send a write packet to the Outstation
            IPAddress addr = IPAddress.Parse(txtBoxWriteIPAddr.Text);
            List<byte> dnpPkt = new List<byte>(); 
            buildPkt(ref dnpPkt, (byte)APDU.functionCode.WRITE);
            byte[] msgBytes = dnpPkt.ToArray();

            //string msg = BitConverter.ToString(msgBytes);
            //Console.WriteLine(msg);  
             
            string response = await runClientWriteAsync(msgBytes, addr);
             
            

            
        }

        public void buildPkt(ref List<byte> dnpPkt, byte fn)
        {
            APDU myApdu = new APDU();
            TPDU myTpdu = new TPDU();

            if (fn == (byte)APDU.functionCode.READ)
            {
                //params should be in the following order for read
                //Confirm, Unsolicited, function, group, variation, prefixQualifier, [range] OR [start index, stop index]
                //myApdu.buildAPDU(ref dnpPkt, 0x00, 0x00, 0x01, 0x0A, 0x01, 0x00, 0x00, 0x02, 0x01, 0x01, 0x01);
                myApdu.buildAPDU(ref dnpPkt, 0x00, 0x00, fn, 0x01, 0x01, 0x00, 0x00, 0x02);
            }
            else if(fn==(byte)APDU.functionCode.WRITE)
            {
                //params should be in the following order for write
                //Confirm, Unsolicited, function, group, variation, prefixQualifier, [range] OR [start index, stop index], [Data-1],[Data-2] etc
                //myApdu.buildAPDU(ref dnpPkt, 0x00, 0x00, 0x01, 0x0A, 0x01, 0x00, 0x00, 0x02, 0x01, 0x01, 0x01);
                myApdu.buildAPDU(ref dnpPkt, 0x00, 0x00, fn, 0x01, 0x01, 0x00, 0x00, 0x02, dataToSend[0], dataToSend[1], dataToSend[2]); //hardcode data for now
            }

            myTpdu.buildTPDU(ref dnpPkt);
            
            DPDU myDpdu = new DPDU();
            myDpdu.buildDPDU(ref dnpPkt, 0xC4, 1, 65519); //dst=1, src=65519
           

        }


        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnReadData_Click(object sender, EventArgs e)
        {

            //client.listenForOutstations(textBox1);

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.CloseSocket(); 
            Environment.Exit(Environment.ExitCode);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //Write values of 011 to the Outstation and read it back 
            dataToSend[0] = 0;
            dataToSend[1] = 1;
            dataToSend[2] = 1;
            bool unitTestCompleted = false;  
            int iterations = 25;
            Stopwatch stopWatch = new Stopwatch(); 
            stopWatch.Start(); 
            for (int i = 0; i < iterations; i++)
            {  
                string testStatus = "Queue Empty" + Environment.NewLine; 
                List<byte> dnpPkt = new List<byte>();
                buildPkt(ref dnpPkt, (byte)APDU.functionCode.WRITE);
                byte[] msgBytes = dnpPkt.ToArray();
                IPAddress addr = IPAddress.Parse(txtBoxWriteIPAddr.Text);
                //string msg = BitConverter.ToString(msgBytes);
                //Console.WriteLine(msg);   
                string response = await runClientWriteAsync(msgBytes, addr);
                textBox1.Text += "Sending Write for Test = " + i + Environment.NewLine;
                dnpPkt.Clear();
                buildPkt(ref dnpPkt, (byte)APDU.functionCode.READ);
                msgBytes = dnpPkt.ToArray();
                response = await runClientWriteAsync(msgBytes, addr);
                textBox1.Text += "Sending Read for Test = " + i + Environment.NewLine; 
                unitTestDone.WaitOne();
                //Thread.Sleep(2000);
                unitTestCompleted = unitTestComplete.TryDequeue(out testStatus);

                if (String.IsNullOrEmpty(testStatus))
                {
                    testStatus = "FAIL: Packet Dropped" + Environment.NewLine;
                }
                

                File.AppendAllText(pathString, testStatus);
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            textBox1.Text += "Test Run Time:  " + elapsedTime + Environment.NewLine;
            double averageResponseTime = ((double)(ts.TotalSeconds) / iterations);
            string responseTime = "Average READ/WRITE Response Time = " + averageResponseTime + Environment.NewLine;
            File.AppendAllText(pathString, (elapsedTime + Environment.NewLine + responseTime + Environment.NewLine));
            stopWatch.Reset();
                
        }

        void checkDNPReqResp()
        {
            string dataWritten, dataRead, unitTestStatus;
            while (true)
            {
                byte[] dataRecvd = new byte[3];
                bool equal = false;
                Console.WriteLine("Waiting for pkt" + Environment.NewLine);
                client.pktToProcess.WaitOne();
                bool dataIn = masterTCPClient.dataFromOutstation.TryDequeue(out dataRecvd);
               
                if (dataIn)
                {
                    string dataInMsg = BitConverter.ToString(dataRecvd);
                    //textBox1.Invoke((MethodInvoker)(() => textBox1.Text += dataInMsg + Environment.NewLine));
                    for (int i = 0; i < 3; i++)
                    {
                        if (dataRecvd[i] == dataToSend[i])
                        {
                            equal = true;
                            continue;
                        }
                        else
                        {
                            equal = false; 
                            break;
                        }
                    }

                    if(equal)
                    {
                         dataWritten = BitConverter.ToString(dataToSend);
                         dataRead = BitConverter.ToString(dataRecvd);
                         unitTestStatus = "PASS: Wrote: " + dataWritten + "  Read: " + dataRead + Environment.NewLine; 
                         
                    }
                    else
                    {
                        dataWritten = BitConverter.ToString(dataToSend);
                        dataRead = BitConverter.ToString(dataRecvd);
                        unitTestStatus = "FAIL: Wrote: " + dataWritten + "  Read: " + dataRead + Environment.NewLine; 
                    }

                    unitTestComplete.Enqueue(unitTestStatus);
                    //textBox1.Invoke((MethodInvoker)(() => textBox1.Text +=unitTestStatus)); 
                    Console.WriteLine(unitTestStatus);
                    unitTestDone.Set();
                }
                else
                {
                    Thread.Sleep(100);
                }
                 
                 
                
            }
                
             
        }



         
    }
}
