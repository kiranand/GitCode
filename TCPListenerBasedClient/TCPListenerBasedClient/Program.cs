using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TCPListenerBasedClient
{
    class Program
    {
        public class RequestSender
        {
          
            public  async Task<string> SendRequest(string data)
            {
                try
                {
                    IPAddress ipAddress = IPAddress.Loopback;  
                
                    TcpClient client = new TcpClient();
                    await client.ConnectAsync(ipAddress, 50000); // Connect
                    NetworkStream networkStream = client.GetStream();
                    StreamWriter writer = new StreamWriter(networkStream);
                    StreamReader reader = new StreamReader(networkStream);
                    writer.AutoFlush = true;
                    await writer.WriteLineAsync(data);
                    string response = await reader.ReadLineAsync();
                    client.Close();
                    return response;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

          

        };

       

        static void Main(string[] args)
        {
            RequestSender rS = new RequestSender();
            string dataToSend = "Cheers";
            Task<string> tsResponse = rS.SendRequest(dataToSend);
            while(true)
            {
                Console.WriteLine("Long Task\n\r");
                Thread.Sleep(2000);
            }
        }
    }
}
