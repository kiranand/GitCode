using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;

namespace FormBasedTCPListenMaster
{
    public class masterTCPClient
    {
        IPAddress OutstationIPAddr;
        byte[] dataRead = new byte[100];
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
                int amountRead = 0;
                 while (!ct.IsCancellationRequested || amountRead==0)
                 {
                     await networkStream.WriteAsync(data, 0, data.Length, ct);

                     amountRead = await networkStream.ReadAsync(dataRead, 0, dataRead.Length, ct);
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
                while (!ct.IsCancellationRequested)
                {

                    var amountRead = await networkStream.ReadAsync(dataRead, 0, dataRead.Length, ct);
                    Console.Write("ReadAsnync returned");
                    //Thread.Sleep(500);
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


        //  TCP SErver COde starts here

        private async Task<List<byte>> readFromClientAsync(TcpClient tcpClient, TextBox tx, CancellationToken ct)
        {
            //string clientEndPoint =
            //tcpClient.Client.RemoteEndPoint.ToString();
            List<byte> clientMsg = new List<byte>();

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

                int amountRead = await networkStream.ReadAsync(dataRead, 0, dataRead.Length, ct);

                if (amountRead > 0)
                {
                    string msgFromClient = BitConverter.ToString(dataRead, 0, amountRead);
                    tx.Text += msgFromClient;

                    for (int i = 0; i < amountRead; i++)
                    {
                        clientMsg.Add(dataRead[i]);
                    }
                    networkStream.Flush();
                }

                //
                // Client closed connection 

                return (clientMsg);
                //tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close();
                return (clientMsg);
            }
        }

        

        public async void listenForOutstations(TextBox txtBx)
        {
            //Make sure to change firewall settings to allow this program to accept connections
            //otherwise it wont work.
            CancellationToken ct;
            List<byte> clientMsg = new List<byte>();
            //IPAddress addr = IPAddress.Parse("192.168.1.200");
            // we are listening to all Master devices on 192.168.1.136 and port 50000
            TcpListener listener = new TcpListener(IPAddress.Any, 30000);
            listener.Start();

            Console.Write("Listening to Outstations" + Environment.NewLine);
            TcpClient tcpClient = new TcpClient();
            while (true)
            {
                try
                {
                    Console.WriteLine("Run"); 
                    tcpClient = await listener.AcceptTcpClientAsync();
                    //ProcessAsync(tcpClient, ct, txtBx); 
                    Console.Write("connection accept");
                    clientMsg = await readFromClientAsync(tcpClient, txtBx, ct);
                    //we got a message from a client, now we process it

                    //await processClientMsgAsync(clientMsg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
            }
        }




    }
}
