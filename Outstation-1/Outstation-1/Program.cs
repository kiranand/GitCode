using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Outstation_1
{
    class Program
    {
        public class AsyncService
        {
            private async Task Process(TcpClient tcpClient)
            {
                string clientEndPoint =
                tcpClient.Client.RemoteEndPoint.ToString();
                Console.WriteLine("Received connection request from " + clientEndPoint);
                try
                {
                    NetworkStream networkStream = tcpClient.GetStream();
                    StreamReader reader = new StreamReader(networkStream);
                    StreamWriter writer = new StreamWriter(networkStream);
                    writer.AutoFlush = true;
                    while (true)
                    {
                        string request = await reader.ReadLineAsync();
                        if (request != null)
                        {
                            Console.WriteLine("Received service request: " + request);
                            //string response = Response(request);
                            Console.WriteLine("Computed response is: " + request + "\n");
                            await writer.WriteLineAsync(request);
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



            public async void Run()
            {
                TcpListener listener = new TcpListener(IPAddress.Any, 50000);
                listener.Start();
                Console.Write("Array Min and Avg service is now running");
                Console.WriteLine(" on port " + 50000);
                Console.WriteLine("Hit <enter> to stop service\n");
                Console.Title = "Outstation-Server";
                while (true)
                {
                    try
                    {
                        TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                        Task t = Process(tcpClient);
                        await t;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
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

        static void Main(string[] args)
        {
            try
            {
                AsyncService service = new AsyncService();
                service.Run();
                Console.ReadLine();
                 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }   

    }
}
