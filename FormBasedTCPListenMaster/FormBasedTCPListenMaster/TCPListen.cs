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
        public async Task<string> SendRequest(string data)
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse("192.168.1.101");

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
    }
}
