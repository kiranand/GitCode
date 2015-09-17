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

        public async Task<string> SendRequest(byte[] data, IPAddress addr, CancellationToken ct)
        {
            byte[] dataRead = new byte[100];
            try
            {
                //IPAddress ipAddress = IPAddress.Parse("192.168.1.123"); 
                //this is the IP address of the Client or Slave running on another computer which is listening on port 50000
                //we need to connect to it and ask for information
                Console.WriteLine();
                TcpClient client = new TcpClient();
                await client.ConnectAsync(addr, 20000); // Connect
                NetworkStream networkStream = client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);
                StreamReader reader = new StreamReader(networkStream);
                writer.AutoFlush = true;
                //await writer.WriteLineAsync(data);
                //string response = await reader.ReadLineAsync();
                 while (!ct.IsCancellationRequested)
                 {
                     await networkStream.WriteAsync(data, 0, data.Length, ct);

                     var amountRead = await networkStream.ReadAsync(dataRead, 0, dataRead.Length, ct);
                 }
               
                client.Close();
                //return response;
                return (dataRead.ToString());
            }

              
            catch (Exception ex)
            {

                return ex.Message;
            }
        }




        public async Task<string> ReadRequest(byte[] data, IPAddress addr)
        {
            byte[] dataRead = new byte[100];
            try
            {
                //IPAddress ipAddress = IPAddress.Parse("192.168.1.123"); 
                //this is the IP address of the Client or Slave running on another computer which is listening on port 50000
                //we need to connect to it and ask for information
                CancellationToken ct;
                Console.WriteLine();
                TcpClient client = new TcpClient();
                await client.ConnectAsync(addr, 20000); // Connect
                NetworkStream networkStream = client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);
                StreamReader reader = new StreamReader(networkStream);
                writer.AutoFlush = true;
                //await writer.WriteLineAsync(data);
                //string response = await reader.ReadLineAsync();
                while (true)
                {

                    var amountRead = await networkStream.ReadAsync(dataRead, 0, dataRead.Length, ct);
                    Thread.Sleep(500);
                }

                client.Close();
                //return response;
                return (dataRead.ToString());
            }


            catch (Exception ex)
            {

                return ex.Message;
            }
        }
    }
}
