using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace DNPMaster_1
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
            Console.Title = "DNP Master Client";
            RequestSender rS = new RequestSender();
            string dataToSend;

            for(int i=0;i<10;i++)
            {
                dataToSend = "Packet:: " + Convert.ToString(i);

                Task<string> tsResponse = rS.SendRequest(dataToSend);
                string reply = tsResponse.Result;
                Console.WriteLine(reply);
            }
            
            Console.ReadLine();
        }
    }
}
 
