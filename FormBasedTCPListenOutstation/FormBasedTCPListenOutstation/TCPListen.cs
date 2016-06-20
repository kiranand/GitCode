using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.WinPcap;
using SharpPcap.AirPcap;
using PacketDotNet;
using System.Net.NetworkInformation;
namespace FormBasedTCPListenOutstation
{

    class TCPListen
    {
        TcpClient tcpClient = new TcpClient();
        List<IPAddress> splitClientList = new List<IPAddress>();
        byte[] splitClientHWaddr = new byte[6];
        IPAddress dsAddr, localAddr; //dsAddr is the address of the station THIS Outstation choses to ask to be the Data server
        IPAddress csAddr; //Connection Server, this is the address of the Connection Server that the Data Server would use to detect DNP pkts to
        //service and reply to - the replies it sends goes to the dnpClientAddr but after changing the self IP addr to the csAddr

        public static ManualResetEvent allDone = new ManualResetEvent(false); 
        RadioButton radio1, radio2, radio3;
        TextBox stationConsole = new TextBox();
        string msgFromClient = "No Data Yet";
        int amountRead = 0;
        int amountWritten = 0;
        bool dataRcvd = false;
        byte[] dataRead = new byte[100];
        DPDU dpdu = new DPDU();
        TPDU tpdu = new TPDU();
        APDU apdu = new APDU();
        
        bool splitProtocol = false;
        bool ispPkt = false;
        SharpPcap.CaptureDeviceList devices;

        public void setLocalAddr(TextBox addresses)
        {
            //first get out IP address and store it for later use
            string str = "";
            stationConsole.Text += "Determine Local Addr" + Environment.NewLine;
           System.Net.NetworkInformation.NetworkInterface[] nics =  System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
           List<string> ipAddr = new List<string>(); 

           foreach (NetworkInterface adapter in nics)
           {
               NetworkInterfaceType networkType = adapter.NetworkInterfaceType;
               if (networkType.Equals(NetworkInterfaceType.Ethernet))
               {

                   foreach (var x in adapter.GetIPProperties().UnicastAddresses)
                   {
                       if (x.Address.AddressFamily == AddressFamily.InterNetwork && x.IsDnsEligible)
                       {
                           Console.WriteLine(" IPAddress ........ : {0:x}", x.Address.ToString());

                           ipAddr.Add(x.Address.ToString());
                       }
                   }

                   PhysicalAddress address = adapter.GetPhysicalAddress();
                   byte[] bytes = address.GetAddressBytes();
                   for (int i = 0; i < bytes.Length; i++)
                   {
                       // Display the physical address in hexadecimal.
                       addresses.Text += bytes[i].ToString("X2");
                       Console.Write("{0}", bytes[i].ToString("X2"));
                       // Insert a hyphen after each byte, unless we are at the end of the 
                       // address.
                       if (i != bytes.Length - 1)
                       {
                           Console.Write("-");
                           addresses.Text += "-";
                       }
                   }
                   addresses.Text += Environment.NewLine;
               }
           }

              
               int count = ipAddr.Count();

               for (int i = 0; i < count; i++)
               {
                   byte[] addrBytes = IPAddress.Parse(ipAddr[i]).GetAddressBytes();

                   if (addrBytes[0] == 0xC0)
                   {
                       localAddr = IPAddress.Parse(ipAddr[i]);
                       addresses.Text += localAddr;
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
                stationConsole.Text+= "No devices were found on this machine" + Environment.NewLine;
                return;
            }

        }

        void sendWithSpoofedAddress(IPAddress addr, byte[] msg)
        {
            
            ushort tcpSourcePort = 30000;
            ushort tcpDestinationPort = 20000; 
            var tcpPacket = new TcpPacket(tcpSourcePort, tcpDestinationPort);
            tcpPacket.PayloadData = msg;
            //var ipSourceAddress = System.Net.IPAddress.Parse("192.168.1.225");
            var ipSourceAddress = addr;
            var ipDestinationAddress = System.Net.IPAddress.Parse(splitClientList[0].ToString());
            var ipPacket = new IPv4Packet(ipSourceAddress, ipDestinationAddress); 
            var sourceHwAddress = "78-E3-B5-57-BC-90";
            var ethernetSourceHwAddress = System.Net.NetworkInformation.PhysicalAddress.Parse(sourceHwAddress);
            //string destinationHwAddress = buildDstHWAddr();
            string destinationHwAddress = BitConverter.ToString(splitClientHWaddr);
            var ethernetDestinationHwAddress = System.Net.NetworkInformation.PhysicalAddress.Parse(destinationHwAddress);
            stationConsole.Text += "Sending Spoofed Msg to Station: " + ipDestinationAddress.ToString() + Environment.NewLine;
            // NOTE: using EthernetPacketType.None to illustrate that the Ethernet
            //       protocol type is updated based on the packet payload that is
            //       assigned to that particular Ethernet packet
            var ethernetPacket = new EthernetPacket(ethernetSourceHwAddress,
                ethernetDestinationHwAddress,
                EthernetPacketType.None);

            // Now stitch all of the packets together
            ipPacket.PayloadPacket = tcpPacket;
            ethernetPacket.PayloadPacket = ipPacket;

            // and print out the packet to see that it looks just like we wanted it to
            Console.WriteLine(ethernetPacket.ToString());

            // to retrieve the bytes that represent this newly created EthernetPacket use the Bytes property
            byte[] packetBytes = ethernetPacket.Bytes;

            // Open the device
            // Extract a device from the list


            // Open the device for capturing
            int readTimeoutMilliseconds = 1000;
            ICaptureDevice device;
            string localInterfaceUsed = "78E3B557BC90";

            for (ushort i = 0; i < 3; i++)
            {  
                device = devices[i]; 
                device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
                var macAddress = device.MacAddress; 
                string hwAddr = macAddress.ToString();
                if (hwAddr.Equals(localInterfaceUsed))
                {
                    try
                    {
                        // Send the packet out the network device
                        device.SendPacket(packetBytes);
                        Console.WriteLine("-- Packet sent successfuly.");
                        device.Close();
                    }
                    catch (Exception eX)
                    {
                        Console.Write("Pkt send failed" + Environment.NewLine);
                    }
                }
            }
        }

        public string buildDstHWAddr()
        {
            string HWAddr;
            StringBuilder builder = new StringBuilder();
            for(int i=0;i<6;i++)
            {
                builder.Append(splitClientHWaddr[i]).Append("-");

            }

            HWAddr = builder.ToString();
            return (HWAddr);
        }

        public void setSplit(bool splitMode)
        {
            splitProtocol = splitMode ? true : false;
        }

        private async void ProcessAsync(TcpClient tcpClient, CancellationToken ct, TextBox txBx)
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

                    await networkStream.WriteAsync(dataRead, 0, dataRead.Length);

                    msgFromClient = BitConverter.ToString(dataRead, 0, amountRead);

                    /*m_TextBox.Invoke(new UpdateTextCallback(this.UpdateText),new object[]{”Text generated on non-UI thread.”});
                     * */

                    txBx.Text += msgFromClient;

                    networkStream.Flush();
                    //
                    // Client closed connection 
                }

                //tcpClient.Close(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    //tcpClient.Close(); 
                    ;
            }
        }

        private async Task<List<byte>> readFromClientAsync(TcpClient tcpClient, CancellationToken ct, TextBox txBx)
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

                amountRead = await networkStream.ReadAsync(dataRead, 0, dataRead.Length, ct);

                if (amountRead > 0)
                {
                    msgFromClient = BitConverter.ToString(dataRead, 0, amountRead);
                    txBx.Text += msgFromClient;

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
                clientMsg.Clear();
                if (tcpClient.Connected)
                    //tcpClient.Close();
                    return (clientMsg);
            }
            return (clientMsg);
        }


        public async Task writeToClient(string msg, TextBox txbx, CancellationToken ct)
        {
            //string clientEndPoint =
            //tcpClient.Client.RemoteEndPoint.ToString();  
            string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
            string localEndPoint = tcpClient.Client.LocalEndPoint.ToString();
            //Console.WriteLine("Received connection request from " + clientEndPoint); 

            Console.WriteLine("WriteToClient Local: " + localEndPoint);
            Console.WriteLine("WriteToClient Remote: " + clientEndPoint);
            TcpClient dataServer = new TcpClient();
            //txbx.Text += "WriteToClient Local: " + localEndPoint;
            txbx.Text += "WriteToClient Remote: " + ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address + Environment.NewLine;
            await dataServer.ConnectAsync(((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address, 30000); // Connect 
            NetworkStream networkStream = dataServer.GetStream();
            StreamReader reader = new StreamReader(networkStream);
            StreamWriter writer = new StreamWriter(networkStream);
            writer.AutoFlush = true;
            byte[] msgToSend = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };

            try
            {
                txbx.Text += "Connected";


                while (!ct.IsCancellationRequested)
                {
                    await networkStream.WriteAsync(msgToSend, 0, msgToSend.Length, ct);
                }

                //dataServer.Close();
                ;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    //tcpClient.Close(); 
                    ;
            }
        }

        public async Task sendReadData(byte[] msg, CancellationToken ct)
        {
            Console.Write("sendReadData" + Environment.NewLine);
            //string clientEndPoint =
            //tcpClient.Client.RemoteEndPoint.ToString();   
            string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
            string localEndPoint = tcpClient.Client.LocalEndPoint.ToString();
            IPAddress addr = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address; ;
            //Console.WriteLine("Received connection request from " + clientEndPoint);  
            stationConsole.Text+= "WriteToClient Local: " + localEndPoint + Environment.NewLine;
            stationConsole.Text+= "WriteToClient Remote: " + clientEndPoint + Environment.NewLine;
            TcpClient dataServer = new TcpClient();
            if (addr.Equals(csAddr)) //change IP address to CS addr before sending this
            {
                string splitClientIPAddrString = splitClientList[0].ToString();
                string csAddrString = csAddr.ToString();
                if (splitClientIPAddrString != null)
                {
                    addr = IPAddress.Parse(csAddrString);
                    sendWithSpoofedAddress(addr, msg);
                }

            }
            else
            {

                await dataServer.ConnectAsync(addr, 20000); // Connect 
                NetworkStream networkStream = dataServer.GetStream();
                try
                {
                    stationConsole.Text += "Connected for SendReadData Client IP = " + addr.ToString() + Environment.NewLine;


                    //while (!ct.IsCancellationRequested)
                    {
                        await networkStream.WriteAsync(msg, 0, msg.Length, ct);
                        stationConsole.Text += "Response" + BitConverter.ToString(msg) + Environment.NewLine;
                    }

                    //dataServer.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (tcpClient.Connected)
                        //tcpClient.Close();
                        ;
                }
            }

          

            stationConsole.Text += "Local Address is now " + localAddr + Environment.NewLine;
        }

        public async Task sendDSRedirect(IPAddress addr, byte[] msg)
        {
            //We need to set the CTRL fn code to DPDU->ISP, i.e. 0x05

            stationConsole.Text += "sendDSRedirect" + Environment.NewLine;

            TcpClient dataServer = new TcpClient();
            stationConsole.Text += "Sending reply to " + addr.ToString() + Environment.NewLine;
            await dataServer.ConnectAsync(addr, 20000); // Connect // Connect 
            NetworkStream networkStream = dataServer.GetStream();
            StreamReader reader = new StreamReader(networkStream);
            StreamWriter writer = new StreamWriter(networkStream);
            writer.AutoFlush = true;
            CancellationToken ct;
            try
            {
                stationConsole.Text += "Connected for DSRedirect" + Environment.NewLine;


                //while (true)
                {
                    await networkStream.WriteAsync(msg, 0, msg.Length, ct);
                }

                //dataServer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    //tcpClient.Close();
                    ;
            }
        }

        public async Task sendISP(byte[] msg, IPAddress addr)
        {
            TcpClient dataServer = new TcpClient();
            await dataServer.ConnectAsync(addr, 20000); // Connect 
            NetworkStream networkStream = dataServer.GetStream();
            StreamReader reader = new StreamReader(networkStream);
            StreamWriter writer = new StreamWriter(networkStream);
            writer.AutoFlush = true;
            CancellationToken ct;
            try
            {

                Console.Write("Connected for SendISP" + Environment.NewLine);


                while (true)
                {
                    await networkStream.WriteAsync(msg, 0, msg.Length, ct);
                }

                //dataServer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SEndISP Exception!");
                if (tcpClient.Connected)
                    //tcpClient.Close();
                    ;
            }
        }

        public async Task buildISP(string dsIpAddr, string splitClientAddr, string hwAddr)
        {
            if (!String.IsNullOrEmpty(dsIpAddr) && !String.IsNullOrEmpty(splitClientAddr) && !String.IsNullOrEmpty(hwAddr)) 
            {
                splitProtocol = true; //set it
                //send an ISP to the Outstation
                try
                {
                    dsAddr = IPAddress.Parse(dsIpAddr); //store this, we need to use this to forward traffic from splitClient to it
                    IPAddress splitClientIP = IPAddress.Parse(splitClientAddr);
                    splitClientList.Add(splitClientIP);
                    APDU ispAPDU = new APDU(); //prepare to respond
                    TPDU ispTPDU = new TPDU();
                    DPDU ispDPDU = new DPDU();
                    List<byte> ispPkt = new List<byte>();
                    string localAddrString = localAddr.ToString();
                    byte[] selfAddrBytes = localAddr.GetAddressBytes();
                    byte[] clientAddrBytes = IPAddress.Parse(splitClientAddr).GetAddressBytes();

                    long value = long.Parse(hwAddr, NumberStyles.HexNumber, CultureInfo.CurrentCulture.NumberFormat);
                    byte[] macBytes = BitConverter.GetBytes(value); 
                    Array.Reverse(macBytes);
                    byte[] macAddress = new byte[6];
                    for (int i = 0; i <= 5; i++)
                        macAddress[i] = macBytes[i + 2];

                    //params should be in the following order
                    //confirm, unsolicited, function, group, variation, prefixQualifier, [range] OR [start index, stop index]
                    ispAPDU.buildAPDU(ref ispPkt, 0x00, 0x00, 0x22, 0x00, 0x00, 0x00, 0x00, 0x0D, selfAddrBytes[0],
                    selfAddrBytes[1], selfAddrBytes[2], selfAddrBytes[3], clientAddrBytes[0], clientAddrBytes[1],
                    clientAddrBytes[2], clientAddrBytes[3], macAddress[0], macAddress[1], macAddress[2], macAddress[3],
                    macAddress[4], macAddress[5]);

                    ispTPDU.buildTPDU(ref ispPkt);
                    ispDPDU.buildDPDU(ref ispPkt, 0xC5, 65519, 1); //dst=1, src=65519
                    byte[] msgBytes = ispPkt.ToArray();
                    string ISPData = BitConverter.ToString(msgBytes); 
                    stationConsole.Text += "Sending ISP :" + ISPData + Environment.NewLine;
                    IPAddress addr = IPAddress.Parse(dsIpAddr);
                    await sendISP(msgBytes, addr);
                }
                catch
                {
                    stationConsole.Text += "ERROR! DS/SplitClient IP Address could not be read" + Environment.NewLine;
                }


            }
            else
            {
                stationConsole.Text += "ERROR!  Need both Data Server and Split Client IP addresses" + Environment.NewLine;
            }
        }



        public async void processAPDURead(List<byte> apduPkt)
        {

            stationConsole.Text += "processAPDURead" + Environment.NewLine;
            CancellationToken ct;
            //We know this is a read, we need to find out what the indices are and return the values
            //we also need to know the Group and Variation to perform the action
            //we need to know the qualifier depending on this we make sense of the range values
            APDU responseAPDU = new APDU(); //prepare to respond
            TPDU responseTPDU = new TPDU();
            DPDU responseDPDU = new DPDU();
            List<byte> dnpResponse = new List<byte>();

            Console.Write("processAPDURead" + Environment.NewLine);
            FormBasedTCPListenOutstation.groupID group = (FormBasedTCPListenOutstation.groupID)apduPkt[2];
            FormBasedTCPListenOutstation.variationID var = (FormBasedTCPListenOutstation.variationID)apduPkt[3];
            FormBasedTCPListenOutstation.prefixAndRange prefixRange = (FormBasedTCPListenOutstation.prefixAndRange)apduPkt[4];

            if (group == FormBasedTCPListenOutstation.groupID.G1)
            {
                if (var == FormBasedTCPListenOutstation.variationID.V1)
                {
                    //this is a group 1 variation 1, report the state of one or more binary points
                    //since this is a read function code, we need to return values of the binary points

                    //now get the qualifier
                    if (prefixRange == FormBasedTCPListenOutstation.prefixAndRange.IndexOneOctetObjectSize)
                    {
                        //this means range field contains one octet count of objects and
                        //each object is preceded by its index

                    }
                    else if (prefixRange == FormBasedTCPListenOutstation.prefixAndRange.NoIndexOneOctetStartStop)
                    {
                        //this means objects are NOT preceded by an index but the range field contains
                        //one start octet with start index value and one stop octet with stop index value
                        //example 00-02-01-01-01  means start index is 00, end index is 02
                        //index 00 = 01, index 01=01 index 02=01
                        byte startIndex = apduPkt[5];
                        byte stopIndex = apduPkt[6];

                        //params should be in the following order
                        //Confirm, Unsolicited, function, group, variation, prefixQualifier, [range] OR [start index, stop index]
                        responseAPDU.buildAPDU(ref dnpResponse, 0x00, 0x00, 0x81, (byte)group, (byte)var, (byte)prefixRange, startIndex, stopIndex, apdu.binaryOutput[0],
                            apdu.binaryOutput[1], apdu.binaryOutput[2]);

                        responseTPDU.buildTPDU(ref dnpResponse);
                        responseDPDU.buildDPDU(ref dnpResponse, 0xC4, 65519, 1); //dst=1, src=65519
                        byte[] msgBytes = dnpResponse.ToArray();

                        string msg = BitConverter.ToString(msgBytes);
                        Console.WriteLine(msg);
                        await sendReadData(msgBytes, ct);
                    }

                }
            }
        }



        public async void processAPDUWrite(List<byte> apduPkt)
        {
            //We know this is a write, we need to find out what the indices are and values
            //we also need to know the Group and Variation to perform the action
            //we need to know the qualifier depending on this we make sense of the range values
            stationConsole.Text += "processAPDUWrite" + Environment.NewLine;
            FormBasedTCPListenOutstation.groupID group = (FormBasedTCPListenOutstation.groupID)apduPkt[2];
            FormBasedTCPListenOutstation.variationID var = (FormBasedTCPListenOutstation.variationID)apduPkt[3];
            FormBasedTCPListenOutstation.prefixAndRange prefixRange = (FormBasedTCPListenOutstation.prefixAndRange)apduPkt[4];

            if (group == FormBasedTCPListenOutstation.groupID.GA)
            {
                if (var == FormBasedTCPListenOutstation.variationID.V1)
                {
                    //this is a group 10 variation 1, control or report the state of one or more binary points
                    //since this is a write function code, we need to write values into the binary points

                    //now get the qualifier
                    if (prefixRange == FormBasedTCPListenOutstation.prefixAndRange.IndexOneOctetObjectSize)
                    {
                        //this means range field contains one octet count of objects and
                        //each object is preceded by its index

                    }
                    else if (prefixRange == FormBasedTCPListenOutstation.prefixAndRange.NoIndexOneOctetStartStop)
                    {
                        //this means objects are NOT preceded by an index but the range field contains
                        //one start octet with start index value and one stop octet with stop index value
                        //example 00-02-01-01-01  means start index is 00, end index is 01
                        //index 00 = 01, index 01=01 index 02=01
                        byte startIndex = apduPkt[5];
                        byte stopIndex = apduPkt[6];
                        int maxObjects = stopIndex - startIndex; //number of values needed
                        int apduCount = apduPkt.Count();// total elements
                        if (apduCount < (7 + maxObjects))
                        {
                            Console.WriteLine("Error, insufficient objects");
                        }
                        else
                        {
                            //get the object values and write it in the database
                            for (byte i = startIndex, j = 0; i <= stopIndex; i++)
                            {
                                apdu.binaryOutput[startIndex + i] = apduPkt[7 + j];
                                j++;
                            }


                            updateRadios(apdu.binaryOutput);
                        }
                    }

                }
            }
        }

        public void updateRadios(byte[] binaryOut)
        {
            radio1.Checked = (binaryOut[0] > 0) ? true : false;
            radio2.Checked = (binaryOut[1] > 0) ? true : false;
            radio3.Checked = (binaryOut[2] > 0) ? true : false;

        }

        public void storeOutstationAddr()
        {
            //lets add the ipaddr of this client into our database for future use
            IPAddress newAddr = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
            if (!splitClientList.Contains(newAddr))
            {
                splitClientList.Add(newAddr);
                foreach (IPAddress addr in splitClientList)
                {
                    stationConsole.Text += "IP:" + addr.ToString() + Environment.NewLine;
                }
            }
        }


        public async void processAPDU(List<byte> apduPkt)
        {
            //process application layer packet
            //CTRL-FNCODE-GRP-VAR-QUAL-RANGE

            //process the CTRL byte which is FIR:FIN:CON:UNS:SEQ
            //we are only interested in the CON bit since that is the only one we are using in an Outstation
            //SEQ is always zero 
            //stationConsole.Text += "processAPDU" + Environment.NewLine;
            //byte control = 0;
            //byte confirmMask = 1 << 5;
            //int unsolicitedMask = 1 << 4;
            //bool confirmSet = ((control & confirmMask) > 0)?true:false;
            //bool unsolicitedSet = ((control & unsolicitedMask) > 0) ? true : false;
            FormBasedTCPListenOutstation.functionCode fnCode;

            if (apduPkt.Count == 0)
            {
                stationConsole.Text += "ERROR!  EMPTY APDU PKT RCVD!" + Environment.NewLine;
            }
            else
            {
                if (apduPkt[1].Equals(0))
                {

                    fnCode = 0;
                }
                else
                {
                    fnCode = (FormBasedTCPListenOutstation.functionCode)apduPkt[1]; //get function code
                }
                stationConsole.Text += "processAPDU fnCode = " + apduPkt[1] + Environment.NewLine;



                switch (fnCode)
                {
                    case FormBasedTCPListenOutstation.functionCode.CONFIRM:
                        //do something
                        break;

                    case FormBasedTCPListenOutstation.functionCode.OPERATE:
                        break;

                    case FormBasedTCPListenOutstation.functionCode.READ:
                        processAPDURead(apduPkt);
                        break;

                    case FormBasedTCPListenOutstation.functionCode.SELECT:
                        break;

                    case FormBasedTCPListenOutstation.functionCode.WRITE:
                        processAPDUWrite(apduPkt);
                        break;

                    case FormBasedTCPListenOutstation.functionCode.ISP:
                        stationConsole.Text += "Recvd ISP" + Environment.NewLine;
                        //splitProtocol = true; //receipt of ISP should not set this, only a transmit from this station should
                        processAPDUIsp(apduPkt);
                        break;
                    default:
                        stationConsole.Text += "processAPDU in default" + Environment.NewLine;
                        break;

                }
            }
        }

        public void processAPDUIsp(List<byte> apduPkt)
        {
            //When we receive this it means we have received a request from an overloaded DNP Outstation to become its
            //Data Server.  We need to store both that Outstation's IP address and the DNP Client it wishes to offload to
            //us.  After this, the overloaded Outstation will redirect all DNP traffic from a client to us and we need to process 
            //and send replies back to that client AFTER changing our IP address to that of the Outstation

            //params should be in the following order
            //confirm[0], unsolicited[1], function[2], group[3], variation[4], prefixQualifier[5], [range[6]] OR [start index, stop index]
            //Our IP addresses will start at byte 7 in the apdu.  First IP addr is the Outstation address and second the splitClient addr
            byte[] CSAddrBytes = { apduPkt[7], apduPkt[8], apduPkt[9], apduPkt[10] };
            byte[] splitClientBytes = { apduPkt[11], apduPkt[12], apduPkt[13], apduPkt[14] };

            for (int i = 0; i < 6;i++ )
            {
                splitClientHWaddr[i] = apduPkt[15 + i];
            }
                csAddr = new IPAddress(CSAddrBytes);
            IPAddress splitClientAddr = new IPAddress(splitClientBytes);
            splitClientList.Add(splitClientAddr);
            stationConsole.Text +=
                "CS Addr = " + csAddr.ToString() + "  " + "SplitClient = " + splitClientAddr.ToString() + Environment.NewLine;
        }



        public async Task<List<byte>> processTPDU(List<byte> tpduPkt)
        {
            Console.Write("processTPDU" + Environment.NewLine);
            List<byte> apduExtract = new List<byte>();

            if (tpduPkt.Count > 0)
            {
                Console.WriteLine("correct");
                for (int i = 1; i < tpduPkt.Count; i++)
                {
                    apduExtract.Add(tpduPkt[i]);
                }

            }

            return (apduExtract);
        }

        public async Task processClientMsgAsync(List<byte> msg)
        {
            //Demultiplex the bytes in msg into layers
            //start with extracting the DPDU
            stationConsole.Text += "processClientMsgAsync" + Environment.NewLine;
            List<byte> dpduPkt = new List<byte>();
            List<byte> tpduPkt = new List<byte>();
            List<byte> apduPkt = new List<byte>();

            if (msg.Count == 0)
            {
                stationConsole.Text += "ERROR!  EMPTY PKT RCVD!" + Environment.NewLine;
            }

            if (msg[0] != 0x05)
            {
                Console.WriteLine("Error not a DNP3 pkt!");
            }
            else
            {
                tpduPkt = await processDPDU(msg);
                if (tpduPkt.Count == 0)
                {
                    stationConsole.Text += "ERROR!  EMPTY APDU PKT RCVD!" + Environment.NewLine;
                }
                else
                {
                    apduPkt = await processTPDU(tpduPkt);
                    if (apduPkt.Count == 0)
                    {
                        stationConsole.Text += "ERROR!  EMPTY APDU PKT RCVD!" + Environment.NewLine;
                    }
                    else
                    {
                        processAPDU(apduPkt);
                    }
                }
                
                
            }



        }


        public async Task<List<byte>> processDPDU(List<byte> dnpPkt)
        {
            Console.Write("processDPDU" + Environment.NewLine);
            //Check the start bytes first
            List<byte> tpduExtract = new List<byte>();
            List<byte> correctedPkt = new List<byte>();
            correctedPkt = checkDuplicates(dnpPkt);
            byte lenBlock2;

            if (correctedPkt[0] == 0x05)
            {
                 
                    //Lets extract only one DNP3 packet here
                    //Check control byte function code
                int controlByte = correctedPkt[3];
                    int fnCodeMask = 0x0000000F;
                    int fnCode = controlByte & fnCodeMask; //last 4 bits
                    if ((DPDU.functionCode)fnCode == DPDU.functionCode.ISP)
                    {
                        ispPkt = true;
                        stationConsole.Text += "Recvd DS Request" + Environment.NewLine;

                    }
                    int userDatalength = correctedPkt[2];
                    userDatalength -= 5; //reduce 5 for header,i.e. CTRL(1)+DST(2)+SRC(2)

                    //Now calculate the header CRC, the length excludes the 2 CRC bytes at the very end
                    //Calculate header CRC
                    byte[] hdrCRC = dpdu.makeCRC(ref correctedPkt, 0, 7);

                    if (correctedPkt[8] == hdrCRC[0] && correctedPkt[9] == hdrCRC[1])
                    {
                        //Now we can proceed to the Block CRC
                        //We are limiting ourselves to <= 16 bytes but it could be greater so check it
                        //first block starts at dnpPkt[10]

                        if (userDatalength > 16)
                        {
                            lenBlock2 = (byte)(userDatalength - (byte)16);
                            //we need two blocks each with its own CRC bytes
                            byte[] crcBlock1 = dpdu.makeCRC(ref correctedPkt, 10, ((10 + 16) - 1));
                            byte[] crcBlock2 = dpdu.makeCRC(ref correctedPkt, (10 + 16 +2), (correctedPkt.Count() - 3));
                            int crcIndex3 = correctedPkt.Count - 2;
                            int crcIndex4 = correctedPkt.Count - 1;
                            int crcIndex1 = correctedPkt.Count - (lenBlock2 + 2 + 2); //2 for CRC of 2nd block and 2 for 1st block CRC
                            int crcIndex2 = crcIndex1 + 1;

                            if((crcBlock1[0]==correctedPkt[crcIndex1]) && (crcBlock1[1]==correctedPkt[crcIndex2]) &&
                                (crcBlock2[0]==correctedPkt[crcIndex3]) && (crcBlock2[1]==correctedPkt[crcIndex4]))
                            {
                                //we can now proceed with processing the data link function code
                                Console.WriteLine("Datalink CRC correct!");
                                //now we can extract the TPDU from the DPDU 

                                for (int i = 10; i < (correctedPkt.Count-2); i++) //start from 10 since first 10 bytes is header
                                {
                                    if(i==crcIndex1)
                                    {
                                        i = i + 2; //skip over first block CRC
                                    } 

                                    tpduExtract.Add(correctedPkt[i]);
                                }
                            }

                        }
                        else
                        {
                            byte[] crcBlock = dpdu.makeCRC(ref correctedPkt, 10, (correctedPkt.Count() - 3));

                            int crcIndex1 = correctedPkt.Count - 2;
                            int crcIndex2 = correctedPkt.Count - 1;

                            if (crcBlock[0] == correctedPkt[crcIndex1] && crcBlock[1] == correctedPkt[crcIndex2])
                            {
                                //we can now proceed with processing the data link function code
                                Console.WriteLine("Datalink CRC correct!");
                                //now we can extract the TPDU from the DPDU 

                                for (int i = 10; i <= (correctedPkt.Count - 2); i++)
                                {
                                    tpduExtract.Add(correctedPkt[i]);
                                }



                            }
                            else
                            {
                                MessageBox.Show("CRC ERROR");
                            }
                        }
                        


                    }
                
            }

            if(tpduExtract.Count==0)
            {
                MessageBox.Show("CRC ERROR");
            }
            return (tpduExtract);
        }


        public List<byte> checkDuplicates(List<byte> pkt)
        {
            //Check if we have more than one dnp3 pkt
            int duplicateIndex = 0;
            bool duplicateFound = false;
            bool pktCorrected = false;
            bool nonDNP3 = false;
            bool emptyPkt = false;
            List<byte> correctedPkt = new List<byte>();

            if (pkt.Count > 0)
            {
                if ((pkt[0] == 0x05) && (pkt[1] == 0x64))
                {
                    int i = 2;
                    for (i = 2; i < pkt.Count; i++)
                    {
                        if ((pkt[i] == 0x05) && (pkt[i + 1] == 0x64))
                        {
                            MessageBox.Show("Duplicate DNP3 Found!");
                            duplicateIndex = i - 1;
                            duplicateFound = true;
                            break;
                        }
                    }

                    if (duplicateFound)
                    {
                        if (duplicateIndex > 0)
                        {
                            //this means we can fix the pkt by dropping the duplicate pkt
                            int k = 0;
                            for (k = 0; k <= duplicateIndex; k++)
                            {
                                correctedPkt.Insert(k, pkt[k]);
                            }
                            pktCorrected = true;
                        } 
                    }

                }
                else
                {
                    nonDNP3 = true;
                }
            }
            else
            {
                emptyPkt = true;
            }

            if(pktCorrected)
            {
                pkt.Clear();
                pkt.AddRange(correctedPkt);
            }
            else if(nonDNP3 || emptyPkt)
            {
                pkt[0] = 0x00;

            }

            return (pkt);
        }



        public async void Run(TextBox txtBx, RadioButton rb1, RadioButton rb2, RadioButton rb3)
        {
            radio1 = rb1;
            radio2 = rb2;
            radio3 = rb3;
            bool msgInProcess = false;
            stationConsole = txtBx;
            apdu.binaryOutput[0] = (radio1.Checked) ? (byte)1 : (byte)0;
            apdu.binaryOutput[1] = (radio1.Checked) ? (byte)1 : (byte)0;
            apdu.binaryOutput[2] = (radio1.Checked) ? (byte)1 : (byte)0;
            CancellationToken ct;
            List<byte> clientMsg = new List<byte>();
            stationConsole.Text += "Outstation Started on " + localAddr.ToString() + Environment.NewLine;
            IPAddress clientIP;
            // we are listening to all Master devices on port 30000
            TcpListener listener = new TcpListener(IPAddress.Any, 20000);
            listener.Start();
            

            stationConsole.Text += " Listening to any IP on port " + 20000 + Environment.NewLine;
            while (true)
            {
                allDone.Reset();
                try
                {
                    Console.WriteLine("Run");
                    Thread.Sleep(10);
                    tcpClient = await listener.AcceptTcpClientAsync();
                    clientIP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
                    txtBx.Text += "Received connection" + Environment.NewLine;
                    clientMsg.Clear(); 
                    clientMsg = await readFromClientAsync(tcpClient, ct, txtBx);
                    stationConsole.Text += "Received Connect from " + clientIP.ToString();
                    List<byte> dnpMsg = new List<byte>();
                    dnpMsg = fixPkt(clientMsg);
                    //we got a message from a client, now we process it
                    if (splitProtocol && splitClientList.Contains(clientIP))
                    {  
                        //tcpClient.Close(); //close existing connection
                        byte[] dsMsg = dnpMsg.ToArray();
                        await sendDSRedirect(dsAddr, dsMsg); //I am redirecting to the Data Server I previously asked to be my Data Server
                         
                    }
                    else
                    { 
                        await processClientMsgAsync(dnpMsg); 
                    }


                    clientMsg.Clear(); //clear the msg

                }
                catch (Exception ex)
                {
                    stationConsole.Text+= "Exception in Run"+Environment.NewLine;

                }
            }


        }

        public List<byte> fixPkt(List<byte> msg)
        {
            //Remove duplicate DNP packets if there are any.
            List<byte> newPkt = new List<byte>();

            var msgArray = msg.ToArray();
            int validIndex = 0;
            for(int i=2;i<(msgArray.Length-1);i++)
            {
                if(msgArray[i]==0x05)
                {
                    if((i+1) < (msgArray.Length-1))
                    {
                        if(msgArray[i+1]==0x64)
                        {
                            //we found a duplicate pkt.  Copy all elements up till (i-1) and return list
                            validIndex = i - 1;
                            break;
                        }
                    }
                }
            }

            if(validIndex > 0)
            {
                for(int j=0;j<=validIndex;j++)
                {
                    newPkt.Insert(j,msgArray[j]);
                }
            }
            else
            {
                validIndex = msgArray.Length - 1;
                for (int j = 0; j <= validIndex; j++)
                {
                    newPkt.Insert(j, msgArray[j]);
                }
            }

            return (newPkt);
        }
         


    }
}

