using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace FormBasedTCPListenMaster
{
    public class TCPClient
    {
        IPAddress OutstationIPAddr;

        public async Task<string> SendRequest(string data, IPAddress addr)
        {
            try
            {
                //IPAddress ipAddress = IPAddress.Parse("192.168.1.123"); 
                //this is the IP address of the Client or Slave running on another computer which is listening on port 50000
                //we need to connect to it and ask for information
                Console.WriteLine();
                TcpClient client = new TcpClient();
                await client.ConnectAsync(addr, 50000); // Connect
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
    }
}
