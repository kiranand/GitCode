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
using System.Collections.Concurrent;

namespace FormBasedTCPListenMaster
{
    public class masterTCPClient
    {
        //public static byte[] dataToSend = new byte[3];
        //public static byte[] dataFromOutStation = new byte[3];

        public static ConcurrentQueue<byte[]> dataToSend = new ConcurrentQueue<byte[]>();
        public static ConcurrentQueue<byte[]> dataFromOutstation = new ConcurrentQueue<byte[]>();

        public IPAddress OutstationIPAddr, localAddr;
        private static object QueueLock = new object(); 
        public static List<byte> dnp3Pkt = new List<byte>(); 
        public static List<byte> pktFromSharpPCap = new List<byte>();
        TcpClient clientToClose;
        TcpListener listenerToStop;
        PhysicalAddress address;
        byte[] dataRead = new byte[100];
        public static TextBox stationConsole = new TextBox();
        SharpPcap.CaptureDeviceList devices;
        bool pktFromSharpPCapRcvd = false;
        public static ICaptureDevice deviceToListen;
        byte[] dataRecvd = new byte[3];
        string filter;
        DPDU dpdu = new DPDU();
        TPDU tpdu = new TPDU();
        APDU apdu = new APDU();
        public EventWaitHandle pktToProcess = new AutoResetEvent(false);

        public async void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {   
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            var tcp = packet.Extract(typeof(TcpPacket)); 

            if (tcp != null)
            {
                DateTime time = e.Packet.Timeval.Date;
                int len = e.Packet.Data.Length;
                var ip = (IpPacket)packet.Extract(typeof(IpPacket));
                string srcIp = ip.SourceAddress.ToString();
                string dstIp = ip.DestinationAddress.ToString();

                if(!(ip.SourceAddress.Equals(localAddr)))
                {   
                    // lock QueueLock to prevent multiple threads accessing PacketQueue at
                    // the same time
                    lock (QueueLock)
                    { 
                        byte[] data = tcp.ParentPacket.Bytes;
                        dnp3Pkt = checkIfDNP3(data);
                        if (dnp3Pkt.Count > 0)
                        {
                            Console.WriteLine("DNP3 PKT RCVD:" + Environment.NewLine); 
                            //string consoleMsg = BitConverter.ToString(dnp3Pkt.ToArray()) + Environment.NewLine;
                            //stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text += consoleMsg));
                            //Console.WriteLine(consoleMsg);
                            pktFromSharpPCapRcvd = true; 
                            Thread.Sleep(100);
                        }
                        
                     
                    }
                    //string dataString = BitConverter.ToString(data);
                    //string dataToSend = "SrcIP = " + srcIp + "  DstIP = " + dstIp + Environment.NewLine +
                    //    "Data = " + dataString + Environment.NewLine;
                    //SetText(dataToSend);
                    //stationConsole.Text += "SrcIP = " + srcIp + " DstIP = " + dstIp + Environment.NewLine;
                    //stationConsole.Text += "Data = " + dataString + Environment.NewLine;  
                    //Console.WriteLine(dataToSend); 
                }

                
            }

          
        }

        private  void BackgroundThread()
        {
           
            while (true)
            {
                bool shouldSleep = true; 
                lock (QueueLock)
                {
                    if (dnp3Pkt.Count() != 0)
                    {
                        shouldSleep = false;
                    }
                }

                if (shouldSleep)
                {
                    System.Threading.Thread.Sleep(100);
                }
                else // should process the queue
                { 
                    lock (QueueLock)
                    {
                        // swap queues, giving the capture callback a new one 
                        pktFromSharpPCap = dnp3Pkt;
                        dnp3Pkt = new List<byte>(); 
                    }

                    Console.WriteLine("BackgroundThread: ourQueue.Count is {0}", pktFromSharpPCap.Count);
                    //stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text += "BackgroundThread: ourQueue.Count is {0}" + 
                      //  pktFromSharpPCap.Count));
                     processClientMsgSync(pktFromSharpPCap);
                  /*  foreach (var packet in ourQueue)
                    {
                        var time = packet.Timeval.Date;
                        var len = packet.Data.Length;
                        Console.WriteLine("BackgroundThread: {0}:{1}:{2},{3} Len={4}",
                            time.Hour, time.Minute, time.Second, time.Millisecond, len);
                    }*/

                    // Here is where we can process our packets freely without
                    // holding off packet capture.
                    //
                    // NOTE: If the incoming packet rate is greater than
                    //       the packet processing rate these queues will grow
                    //       to enormous sizes. Packets should be dropped in these
                    //       cases

                }
            }
        }

       
        List<byte> checkIfDNP3(byte[] pkt)
        {
            List<byte> dnp3Pkt = new List<byte>();

            for(int i=0;((i<pkt.Count()) && ((i+1) < pkt.Count()));i++)
            {
                if((pkt[i]==0x05) && (pkt[i+1]==0x64))
                {
                    for(int j=i;j<pkt.Count();j++)
                    {
                        dnp3Pkt.Add(pkt[j]);
                    }
                    break;
                }
                else
                {
                    dnp3Pkt.Clear();
                }
            }

            return (dnp3Pkt);

        }

        public void setLocalAddr(TextBox txBx)
        {
            stationConsole = txBx;
            //first get out IP address and store it for later use
            string str = "";
            //stationConsole.Text += "Determine Local Addr" + Environment.NewLine;
            System.Net.NetworkInformation.NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
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

                    address = adapter.GetPhysicalAddress();
                    byte[] bytes = address.GetAddressBytes();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        // Display the physical address in hexadecimal.
                        txBx.Text += bytes[i].ToString("X2");
                        Console.Write("{0}", bytes[i].ToString("X2"));
                        // Insert a hyphen after each byte, unless we are at the end of the 
                        // address.
                        if (i != bytes.Length - 1)
                        {
                            Console.Write("-");
                            txBx.Text += "-";
                        }
                    }
                    txBx.Text += Environment.NewLine;
                }
            }


            int count = ipAddr.Count();

            for (int i = 0; i < count; i++)
            {
                byte[] addrBytes = IPAddress.Parse(ipAddr[i]).GetAddressBytes();

                if (addrBytes[0] == 0xC0)
                {
                    localAddr = IPAddress.Parse(ipAddr[i]);
                    txBx.Text += localAddr;
                    break;
                }
                else
                {
                    stationConsole.Text += "ERROR Unable to determine self IP address!!!" + Environment.NewLine;
                }
            }

            // Print SharpPcap version 
            string ver = SharpPcap.Version.VersionString;
            //stationConsole.Text += "SharpPcap {0}, Example1.IfList.cs" + ver + Environment.NewLine;

            // Retrieve the device list 
            devices = SharpPcap.CaptureDeviceList.Instance;

            // If no devices were found print an error
            if (devices.Count < 1)
            {
                stationConsole.Text += "No devices were found on this machine" + Environment.NewLine;
                return;
            }
            else
            {
                //stationConsole.Text += "Device count = " + devices.Count + Environment.NewLine;
            }

            //Console.WriteLine("\nThe following devices are available on this machine:");
            //Console.WriteLine("----------------------------------------------------\n");

            // Print out the available network devices
            //foreach (ICaptureDevice dev in devices)
            //    Console.WriteLine("{0}\n", dev.ToString()); 
            // Open the device for capturing
           

        }
        
        

        public async Task<List<byte>> getPktFromInterfaceAsync()
        {
            List<byte> dnp3Pkt = new List<byte>();
            var rawPacket = deviceToListen.GetNextPacket();
            var packet = PacketDotNet.Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
            return await Task.Factory.StartNew(()=>
            {
                while (packet != null)
                {
                    var tcp = packet.Extract(typeof(TcpPacket)); 

                    if (tcp != null)
                    { 
                        var ip = (IpPacket)packet.Extract(typeof(IpPacket));
                        string srcIp = ip.SourceAddress.ToString();
                        string dstIp = ip.DestinationAddress.ToString();
                        byte[] data = tcp.ParentPacket.Bytes;
                        if (!(ip.SourceAddress.Equals(localAddr)))
                        {
                            dnp3Pkt = checkIfDNP3(data);
                            if (dnp3Pkt.Count() > 0)
                            {
                                Console.WriteLine("DNP3 PKT RCVD:" + Environment.NewLine); 
                            }
                            string dataString = BitConverter.ToString(data);
                            string dataToSend = "SrcIP = " + srcIp + "  DstIP = " + dstIp + Environment.NewLine +
                                "Data = " + dataString + Environment.NewLine;
                            //SetText(dataToSend);
                            //stationConsole.Text += "SrcIP = " + srcIp + " DstIP = " + dstIp + Environment.NewLine;
                            //stationConsole.Text += "Data = " + dataString + Environment.NewLine;  
                            Console.WriteLine(dataToSend);
                        }


                    }
                }

                return (dnp3Pkt);
            });
        }

        public async Task<string> SendRequest(byte[] data, IPAddress addr, CancellationToken ct)
        {
            byte[] dataRead = new byte[100];
            try
            {
                //IPAddress ipAddress = IPAddress.Parse("192.168.1.123"); 
                //this is the IP address of the Client or Slave running on another computer which is listening on port 50000
                //we need to connect to it and ask for information 
                TcpClient client = new TcpClient();
                await client.ConnectAsync(addr, 20000); // Connect 
                NetworkStream networkStream = client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);
                StreamReader reader = new StreamReader(networkStream);
                writer.AutoFlush = true;
                //await writer.WriteLineAsync(data);
                //string response = await reader.ReadLineAsync();
                int amountRead = 0;
                 /*while (!ct.IsCancellationRequested || amountRead==0)
                 {
                     await networkStream.WriteAsync(data, 0, data.Length, ct);

                     //amountRead = await networkStream.ReadAsync(dataRead, 0, dataRead.Length, ct);
                 }*/


                await networkStream.WriteAsync(data, 0, data.Length, ct);
                //client.Close();
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

                //client.Close();
                //return response;
                return (dataRead.ToString());
            }


            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        
        public async void listenForOutstations(TextBox txBx)
        {
            stationConsole = txBx;
            //Make sure to change firewall settings to allow this program to accept connections
            //otherwise it wont work.
            CancellationToken ct; 
            //IPAddress addr = IPAddress.Parse("192.168.1.200");
            // we are listening to all Master devices on 192.168.1.136 and port 50000
            TcpListener listener = new TcpListener(IPAddress.Any, 20000);
            listenerToStop = listener;
            listener.Start();
            int readTimeoutMilliseconds = 1000;
            ICaptureDevice device;
            filter = "port 20000";
            string localInterfaceUsed = address.ToString();

            for (ushort i = 0; i < devices.Count; i++)
            {

                device = devices[i];

                // Register our handler function to the
                // 'packet arrival' event
                device.OnPacketArrival +=
                  new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival); 
                device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
                var macAddress = device.MacAddress;
                string hwAddr = macAddress.ToString();
                if (hwAddr.Equals(localInterfaceUsed))
                {
                    deviceToListen = device;
                    stationConsole.Text += "DEVICE INSTALLED" + Environment.NewLine;
                    break;
                }
                else
                {
                    stationConsole.Text += "DEVICE NOT READY!!" + Environment.NewLine;
                }
            }
            
            //stationConsole.Text += "Listening on " + deviceToListen.ToString() + Environment.NewLine;
            txBx.Text += "Listening to Outstations" + Environment.NewLine;
            TcpClient tcpClient = new TcpClient();
            clientToClose = tcpClient;
            List<byte> clientMsg = new List<byte>(); 
            
                try
                {
                    Console.WriteLine("Run");
                    deviceToListen.StartCapture();
                    var backgroundThread = new System.Threading.Thread(BackgroundThread);
                    backgroundThread.Start();
                    tcpClient = await listener.AcceptTcpClientAsync();
                    //ProcessAsync(tcpClient, ct, txtBx); 
                    //txtBx.Text += "connection accept" + Environment.NewLine;
                     
                    //we got a message from a client, now we process it
                   

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

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
            FormBasedTCPListenMaster.APDU.functionCode fnCode;

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
                    fnCode = (FormBasedTCPListenMaster.APDU.functionCode)apduPkt[1]; //get function code
                }
                stationConsole.Text += "processAPDU fnCode = " + apduPkt[1] + Environment.NewLine; 

                switch (fnCode)
                {
                    case FormBasedTCPListenMaster.APDU.functionCode.CONFIRM:
                        //do something
                        break;

                    case FormBasedTCPListenMaster.APDU.functionCode.OPERATE:
                        break;

                    case FormBasedTCPListenMaster.APDU.functionCode.READ:
                        processAPDURead(apduPkt);
                        break;

                    case FormBasedTCPListenMaster.APDU.functionCode.SELECT:
                        break;

                    case FormBasedTCPListenMaster.APDU.functionCode.WRITE:
                        processAPDUWrite(apduPkt);
                        break;

                    default:
                        stationConsole.Text += "processAPDU in default" + Environment.NewLine;
                        break;

                }
            }
        }

        public void processAPDUSync(List<byte> apduPkt)
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
            FormBasedTCPListenMaster.APDU.functionCode fnCode;

            if (apduPkt.Count == 0)
            {
                
               Console.WriteLine("ERROR!  EMPTY APDU PKT RCVD!" + Environment.NewLine);
            }
            else
            {
                if (apduPkt[1].Equals(0))
                {

                    fnCode = 0;
                }
                else
                {
                    fnCode = (FormBasedTCPListenMaster.APDU.functionCode)apduPkt[1]; //get function code
                }
                Console.WriteLine("processAPDU fnCode = " + apduPkt[1] + Environment.NewLine);

                switch (fnCode)
                {
                    case FormBasedTCPListenMaster.APDU.functionCode.CONFIRM:
                        //do something
                        break;

                    case FormBasedTCPListenMaster.APDU.functionCode.OPERATE:
                        break;

                    case FormBasedTCPListenMaster.APDU.functionCode.READ:
                        processAPDURead(apduPkt);
                        break;

                    case FormBasedTCPListenMaster.APDU.functionCode.SELECT:
                        break;

                    case FormBasedTCPListenMaster.APDU.functionCode.WRITE:
                        processAPDUWrite(apduPkt);
                        break;

                    case FormBasedTCPListenMaster.APDU.functionCode.RESPONSE:
                        //stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text = "Process DNP Response" + Environment.NewLine));
                        processAPDUResponse(apduPkt);
                        break;

                    default:
                        Console.WriteLine("processAPDU in default" + Environment.NewLine);
                        break;

                }
            }
        }

         
        public void processAPDUResponse(List<byte> apduPkt)
        {
            byte[] data = new byte[3];
            int mask = 1;
            byte dataByte;
            if(apduPkt.Count > 0)
            {
                //Skip past function code and IIN octets
                //APDU structure is [CONTROL][FNCODE][IIN.1][IIN.2][GRP][VAR][PREFIX-RANGE][START-INDEX][STOP-INDEX][DATA]
                 byte GRP = apduPkt[4];
                 byte VAR = apduPkt[5];
                 byte RANGE = apduPkt[6];
                 byte START = apduPkt[7];
                 byte STOP = apduPkt[8];
                 dataByte = apduPkt[9];

                for(int i=0;i<=(STOP-START);i++)
                { 
                    //Now extract the binary data and convert to byte
                    int result = ((mask << i) & dataByte);
                    result = result >> i;
                    data[i] = (result > 0) ? (byte)1 : (byte)0;
                }

                dataFromOutstation.Enqueue(data);
                Console.WriteLine("Pkt Enqueued" + Environment.NewLine);
                pktToProcess.Set(); 
            }
            else
            {   
                stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text = "ERROR EMPTY APDU!" + Environment.NewLine));
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
                        byte[] crcBlock2 = dpdu.makeCRC(ref correctedPkt, (10 + 16 + 2), (correctedPkt.Count() - 3));
                        int crcIndex3 = correctedPkt.Count - 2;
                        int crcIndex4 = correctedPkt.Count - 1;
                        int crcIndex1 = correctedPkt.Count - (lenBlock2 + 2 + 2); //2 for CRC of 2nd block and 2 for 1st block CRC
                        int crcIndex2 = crcIndex1 + 1;

                        if ((crcBlock1[0] == correctedPkt[crcIndex1]) && (crcBlock1[1] == correctedPkt[crcIndex2]) &&
                            (crcBlock2[0] == correctedPkt[crcIndex3]) && (crcBlock2[1] == correctedPkt[crcIndex4]))
                        {
                            //we can now proceed with processing the data link function code
                            Console.WriteLine("Datalink CRC correct!");
                            //now we can extract the TPDU from the DPDU 

                            for (int i = 10; i < (correctedPkt.Count - 2); i++) //start from 10 since first 10 bytes is header
                            {
                                if (i == crcIndex1)
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

            if (tpduExtract.Count == 0)
            {
                MessageBox.Show("CRC ERROR");
            }
            return (tpduExtract);
        }

        public List<byte> processDPDUSync(List<byte> dnpPkt)
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
                        byte[] crcBlock2 = dpdu.makeCRC(ref correctedPkt, (10 + 16 + 2), (correctedPkt.Count() - 3));
                        int crcIndex3 = correctedPkt.Count - 2;
                        int crcIndex4 = correctedPkt.Count - 1;
                        int crcIndex1 = correctedPkt.Count - (lenBlock2 + 2 + 2); //2 for CRC of 2nd block and 2 for 1st block CRC
                        int crcIndex2 = crcIndex1 + 1;

                        if ((crcBlock1[0] == correctedPkt[crcIndex1]) && (crcBlock1[1] == correctedPkt[crcIndex2]) &&
                            (crcBlock2[0] == correctedPkt[crcIndex3]) && (crcBlock2[1] == correctedPkt[crcIndex4]))
                        {
                            //we can now proceed with processing the data link function code
                            Console.WriteLine("Datalink CRC correct!");
                            //now we can extract the TPDU from the DPDU 

                            for (int i = 10; i < (correctedPkt.Count - 2); i++) //start from 10 since first 10 bytes is header
                            {
                                if (i == crcIndex1)
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

            if (tpduExtract.Count == 0)
            {
                MessageBox.Show("CRC ERROR");
            }
            return (tpduExtract);
        }

        public void CloseSocket()
        {
            clientToClose.Close();
            listenerToStop.Stop();
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

            if (pktCorrected)
            {
                pkt.Clear();
                pkt.AddRange(correctedPkt);
            }
            else if (nonDNP3 || emptyPkt)
            {
                pkt[0] = 0x00;

            }

            return (pkt);
        }

        public async Task processClientMsgAsync(List<byte> msg)
        {
            //Demultiplex the bytes in msg into layers
            //start with extracting the DPDU
           
                if (pktFromSharpPCapRcvd == true)
                {    
                    stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text +=
                           "processClientMsgAsync" + Environment.NewLine));

                    List<byte> dpduPkt = new List<byte>();
                    List<byte> tpduPkt = new List<byte>();
                    List<byte> apduPkt = new List<byte>();

                    if (msg.Count == 0)
                    { 
                        stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text += 
                            "ERROR!  EMPTY PKT RCVD!" + Environment.NewLine));
                    }

                    if (msg[0] != 0x05)
                    {
                        Console.WriteLine("Error not a DNP3 pkt!");
                    }
                    else
                    {
                        tpduPkt =  await processDPDU(msg);
                        if (tpduPkt.Count == 0)
                        {
                            stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text +=
                                "ERROR!  EMPTY APDU PKT RCVD!" + Environment.NewLine));

                        }
                        else
                        {
                            apduPkt = await processTPDU(tpduPkt);
                            if (apduPkt.Count == 0)
                            {
                                stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text +=
                                    "ERROR!  EMPTY APDU PKT RCVD!" + Environment.NewLine));
                            }
                            else
                            {
                                processAPDU(apduPkt);
                            }
                        } 
                    }

                    deviceToListen.StartCapture();
                } 

        }

        public void processClientMsgSync(List<byte> msg)
        {
            //Demultiplex the bytes in msg into layers
            //start with extracting the DPDU

            if (pktFromSharpPCapRcvd == true)
            {
                
                List<byte> dpduPkt = new List<byte>();
                List<byte> tpduPkt = new List<byte>();
                List<byte> apduPkt = new List<byte>();

                if (msg.Count == 0)
                {
                    //stationConsole.Text += "ERROR!  EMPTY PKT RCVD!" + Environment.NewLine;
                    stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text = "ERROR!  EMPTY PKT RCVD!" + Environment.NewLine));
                }

                if (msg[0] != 0x05)
                {
                    Console.WriteLine("Error not a DNP3 pkt!");
                }
                else
                {
                    tpduPkt = processDPDUSync(msg);
                    if (tpduPkt.Count == 0)
                    {
                        //stationConsole.Text += "ERROR!  EMPTY APDU PKT RCVD!" + Environment.NewLine;
                        stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text = "ERROR!  EMPTY APDU PKT RCVD!" + Environment.NewLine));
                    }
                    else
                    {
                        apduPkt = processTPDUSync(tpduPkt);
                        if (apduPkt.Count == 0)
                        {
                            //stationConsole.Text += "ERROR!  EMPTY APDU PKT RCVD!" + Environment.NewLine;
                            stationConsole.Invoke((MethodInvoker)(() => stationConsole.Text = 
                                "ERROR!  EMPTY APDU PKT RCVD!" + Environment.NewLine));
                        }
                        else
                        {
                            processAPDUSync(apduPkt);
                        }
                    }


                } 
            }

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

        public List<byte> processTPDUSync(List<byte> tpduPkt)
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

        public void processAPDURead(List<byte> apduPkt)
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
            FormBasedTCPListenMaster.APDU.groupID group = (FormBasedTCPListenMaster.APDU.groupID)apduPkt[2];
            FormBasedTCPListenMaster.APDU.variationID var = (FormBasedTCPListenMaster.APDU.variationID)apduPkt[3];
            FormBasedTCPListenMaster.APDU.prefixAndRange prefixRange = (FormBasedTCPListenMaster.APDU.prefixAndRange)apduPkt[4];

            if (group == FormBasedTCPListenMaster.APDU.groupID.G1)
            {
                if (var == FormBasedTCPListenMaster.APDU.variationID.V1)
                {
                    //this is a group 1 variation 1, report the state of one or more binary points
                    //since this is a read function code, we need to return values of the binary points

                    //now get the qualifier
                    if (prefixRange == FormBasedTCPListenMaster.APDU.prefixAndRange.IndexOneOctetObjectSize)
                    {
                        //this means range field contains one octet count of objects and
                        //each object is preceded by its index

                    }
                    else if (prefixRange == FormBasedTCPListenMaster.APDU.prefixAndRange.NoIndexOneOctetStartStop)
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
                        stationConsole.Text += msg + Environment.NewLine;
                        //await sendReadData(msgBytes, ct);
                    }

                }
            }
        }

        public void processAPDUReadSync(List<byte> apduPkt)
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
            FormBasedTCPListenMaster.APDU.groupID group = (FormBasedTCPListenMaster.APDU.groupID)apduPkt[2];
            FormBasedTCPListenMaster.APDU.variationID var = (FormBasedTCPListenMaster.APDU.variationID)apduPkt[3];
            FormBasedTCPListenMaster.APDU.prefixAndRange prefixRange = (FormBasedTCPListenMaster.APDU.prefixAndRange)apduPkt[4];

            if (group == FormBasedTCPListenMaster.APDU.groupID.G1)
            {
                if (var == FormBasedTCPListenMaster.APDU.variationID.V1)
                {
                    //this is a group 1 variation 1, report the state of one or more binary points
                    //since this is a read function code, we need to return values of the binary points

                    //now get the qualifier
                    if (prefixRange == FormBasedTCPListenMaster.APDU.prefixAndRange.IndexOneOctetObjectSize)
                    {
                        //this means range field contains one octet count of objects and
                        //each object is preceded by its index

                    }
                    else if (prefixRange == FormBasedTCPListenMaster.APDU.prefixAndRange.NoIndexOneOctetStartStop)
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
                        stationConsole.Text += msg + Environment.NewLine;
                        //await sendReadData(msgBytes, ct);
                    }

                }
            }
        }

        public void processAPDUWrite(List<byte> apduPkt)
        {
            //We know this is a write, we need to find out what the indices are and values
            //we also need to know the Group and Variation to perform the action
            //we need to know the qualifier depending on this we make sense of the range values
            stationConsole.Text += "processAPDUWrite" + Environment.NewLine;
            FormBasedTCPListenMaster.APDU.groupID group = (FormBasedTCPListenMaster.APDU.groupID)apduPkt[2];
            FormBasedTCPListenMaster.APDU.variationID var = (FormBasedTCPListenMaster.APDU.variationID)apduPkt[3];
            FormBasedTCPListenMaster.APDU.prefixAndRange prefixRange = (FormBasedTCPListenMaster.APDU.prefixAndRange)apduPkt[4];

            if (group == FormBasedTCPListenMaster.APDU.groupID.GA)
            {
                if (var == FormBasedTCPListenMaster.APDU.variationID.V1)
                {
                    //this is a group 10 variation 1, control or report the state of one or more binary points
                    //since this is a write function code, we need to write values into the binary points

                    //now get the qualifier
                    if (prefixRange == FormBasedTCPListenMaster.APDU.prefixAndRange.IndexOneOctetObjectSize)
                    {
                        //this means range field contains one octet count of objects and
                        //each object is preceded by its index

                    }
                    else if (prefixRange == FormBasedTCPListenMaster.APDU.prefixAndRange.NoIndexOneOctetStartStop)
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


                            //updateRadios(apdu.binaryOutput);
                        }
                    }

                }
            }
        } //processAPDUWrite


    }
}
