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
using System.Diagnostics;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.WinPcap;
using SharpPcap.AirPcap;
using PacketDotNet;
using System.Net.NetworkInformation;

namespace FormBasedTCPListenMaster
{
    public class masterTCPClient
    {
        public IPAddress OutstationIPAddr, localAddr;
        byte[] dataRead = new byte[100];
        TextBox stationConsole = new TextBox();
        SharpPcap.CaptureDeviceList devices;
        public void setLocalAddr(TextBox txBx)
        {
            stationConsole = txBx;
            //first get out IP address and store it for later use
            string str = "";
            stationConsole.Text += "Determine Local Addr" + Environment.NewLine;
            System.Net.NetworkInformation.NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            List<string> ipAddr = new List<string>();

            foreach (NetworkInterface adapter in nics)
            {
                foreach (var x in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (x.Address.AddressFamily == AddressFamily.InterNetwork && x.IsDnsEligible)
                    {
                        Console.WriteLine(" IPAddress ........ : {0:x}", x.Address.ToString());
                        ipAddr.Add(x.Address.ToString());
                    }
                }
            }


            int count = ipAddr.Count();

            for (int i = 0; i < count; i++)
            {
                byte[] addrBytes = IPAddress.Parse(ipAddr[i]).GetAddressBytes();

                if (addrBytes[0] == 0xC0)
                {
                    localAddr = IPAddress.Parse(ipAddr[i]);
                    break;
                }
                else
                {
                    stationConsole.Text += "ERROR Unable to determine self IP address!!!" + Environment.NewLine;
                }
            }

            // Retrieve the device list

            devices = SharpPcap.CaptureDeviceList.Instance;

            // If no devices were found print an error
            if (devices.Count < 1)
            {
                stationConsole.Text += "No devices were found on this machine" + Environment.NewLine;
                return;
            }

        }


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

                     //amountRead = await networkStream.ReadAsync(dataRead, 0, dataRead.Length, ct);
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
         
            // Get the elapsed time as a TimeSpan value.
            
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
                    tx.Text += "Read Response" + Environment.NewLine + msgFromClient + Environment.NewLine;

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
            TcpListener listener = new TcpListener(IPAddress.Any, 20000);
            listener.Start();

            txtBx.Text += "Listening to Outstations" + Environment.NewLine;
            TcpClient tcpClient = new TcpClient();
            while (true)
            {
                try
                {
                    Console.WriteLine("Run"); 
                    tcpClient = await listener.AcceptTcpClientAsync();
                    //ProcessAsync(tcpClient, ct, txtBx); 
                    txtBx.Text += "connection accept" + Environment.NewLine;
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
