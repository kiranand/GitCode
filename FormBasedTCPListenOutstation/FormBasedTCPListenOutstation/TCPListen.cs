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
        private async Task Process(TcpClient tcpClient)
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
                        string msgRcvd = await reader.ReadLineAsync();
                        Console.WriteLine("Waiting for client data");
                        if (msgRcvd != null)
                        {
                            Console.WriteLine("Received service request: " + msgRcvd);
                            msgFromClient = string.Copy(msgRcvd);
                            //string response = Response(request);
                            Console.WriteLine("Computed response is: " + msgRcvd + "\n");

                            await writer.WriteLineAsync(msgRcvd);
                        }
                        else
                             
                            break; // Client closed connection
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



            public async Task<string> Run()
            {
                IPAddress self = IPAddress.Parse("127.0.0.1"); 
                // we are listening to all Master devices on 192.168.1.136 and port 50000
                TcpListener listener = new TcpListener(IPAddress.Any, 50000);
                listener.Start(); 
                
                Console.Write("Array Min and Avg service is now running");
                Console.WriteLine(" on port " + 50000);
                while (true)
                {
                    try
                    {
                        TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                        Task t = Process(tcpClient);
                        await t;
                        return (msgFromClient);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return (msgFromClient);
                    }
                }

                
            }

            private static string Response(string request)
            {
                string dataToSend = "Packet Received";
                System.Threading.Thread.Sleep(1000);
                return dataToSend;
            }
            
        }
    }

