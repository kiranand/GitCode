using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace FormBasedTCPListenOutstation
{
    class TCPListen
    {
        string msgFromClient = "No Data Yet";
        int amountRead = 0;
        bool dataRcvd = false;
        byte[] dataRead = new byte[100];

        private async void ProcessAsync(TcpClient tcpClient, CancellationToken ct)
        {
            //string clientEndPoint =
            //tcpClient.Client.RemoteEndPoint.ToString();
                 
            string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
            string localEndPoint = tcpClient.Client.LocalEndPoint.ToString();
            //Console.WriteLine("Received connection request from " + clientEndPoint); 
            Console.WriteLine("Local: " + localEndPoint);
            Console.WriteLine("Remote: " + clientEndPoint);

            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream);
                StreamWriter writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;
               
                while (true)
                {
                    amountRead = await networkStream.ReadAsync(dataRead, 0, dataRead.Length, ct);

                    if (amountRead <= 0)
                    {
                        break;
                        
                    }
                    
                    msgFromClient = BitConverter.ToString(dataRead);
                    Console.WriteLine("Received service request: " + msgFromClient); 
                    networkStream.Flush();
                        
                    //
                    // Client closed connection 
                }
                    
                tcpClient.Close(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close(); 
            }
        }



            public async void Run()
            {
                CancellationToken ct;
                IPAddress self = IPAddress.Parse("127.0.0.1"); 
                // we are listening to all Master devices on 192.168.1.136 and port 50000
                TcpListener listener = new TcpListener(IPAddress.Any, 20000);
                listener.Start(); 
                
                Console.Write("Array Min and Avg service is now running");
                Console.WriteLine(" on port " + 50000);
                while (true)
                {
                    try
                    { 
                        Console.WriteLine("Run");
                        TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                        ProcessAsync(tcpClient, ct); 
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                }

                
            }

            public string Response()
            {
               if(!dataRcvd)
               {
                   msgFromClient = null;
               }
               return(msgFromClient);
            }
            
        }
    }

