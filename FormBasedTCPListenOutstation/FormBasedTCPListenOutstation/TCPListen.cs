﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading; 
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;
 

namespace FormBasedTCPListenOutstation
{
   
    class TCPListen
    {
        TcpClient tcpClient = new TcpClient();
        List<IPAddress> dnpClientAddr = new List<IPAddress>();
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
                  
                    msgFromClient = BitConverter.ToString(dataRead,0,amountRead);

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

                if(amountRead > 0)
                {
                    msgFromClient = BitConverter.ToString(dataRead, 0, amountRead);
                    txBx.Text += msgFromClient;

                    for (int i = 0; i < amountRead;i++)
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
            await dataServer.ConnectAsync(((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address,30000); // Connect 
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

        public async Task sendReadData(byte[] msg)
        {
            Console.Write("sendReadData" + Environment.NewLine);
            //string clientEndPoint =
            //tcpClient.Client.RemoteEndPoint.ToString();   
            string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
            string localEndPoint = tcpClient.Client.LocalEndPoint.ToString();
            //Console.WriteLine("Received connection request from " + clientEndPoint);  
            Console.WriteLine("WriteToClient Local: " + localEndPoint);
            Console.WriteLine("WriteToClient Remote: " + clientEndPoint);
            TcpClient dataServer = new TcpClient(); 
            await dataServer.ConnectAsync(((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address, 30000); // Connect 
            NetworkStream networkStream = dataServer.GetStream();
            StreamReader reader = new StreamReader(networkStream);
            StreamWriter writer = new StreamWriter(networkStream);
            writer.AutoFlush = true; 
            CancellationToken ct;
            try
            {
                Console.Write("Connected for SendReadData" + Environment.NewLine);


                while (true)
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

        public async Task buildISP(string dsIpAddr)
        {
            //send an ISP to the Outstation
            IPAddress dsAddr = IPAddress.Parse(dsIpAddr);
            APDU ispAPDU = new APDU(); //prepare to respond
            TPDU ispTPDU = new TPDU();
            DPDU ispDPDU = new DPDU();
            List<byte> ispPkt = new List<byte>();
            ispAPDU.buildAPDU(ref ispPkt, 0x00, 0x00, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00);

            ispTPDU.buildTPDU(ref ispPkt);
            ispDPDU.buildDPDU(ref ispPkt, 0xC5, 65519, 1); //dst=1, src=65519
            byte[] msgBytes = ispPkt.ToArray(); 
            stationConsole.Text += "Sending ISP" + Environment.NewLine;
            IPAddress addr = IPAddress.Parse(dsIpAddr);
            await sendISP(msgBytes, addr); 
        }

       

        public async void processAPDURead(List<byte> apduPkt)
        {
            //First check to see if we are doing split protocol
            if(splitProtocol)
            {

            }
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
                        await sendReadData(msgBytes); 
                    }

                }
            }
        }


        
        public async void processAPDUWrite(List<byte> apduPkt)
        {
            //We know this is a write, we need to find out what the indices are and values
            //we also need to know the Group and Variation to perform the action
            //we need to know the qualifier depending on this we make sense of the range values
            Console.Write("processAPDUWrite" + Environment.NewLine);
            FormBasedTCPListenOutstation.groupID group = (FormBasedTCPListenOutstation.groupID)apduPkt[2];
            FormBasedTCPListenOutstation.variationID var = (FormBasedTCPListenOutstation.variationID)apduPkt[3];
            FormBasedTCPListenOutstation.prefixAndRange prefixRange = (FormBasedTCPListenOutstation.prefixAndRange)apduPkt[4];

            if(group==FormBasedTCPListenOutstation.groupID.GA)
            {
                if(var==FormBasedTCPListenOutstation.variationID.V1)
                {
                    //this is a group 10 variation 1, control or report the state of one or more binary points
                    //since this is a write function code, we need to write values into the binary points

                    //now get the qualifier
                    if(prefixRange==FormBasedTCPListenOutstation.prefixAndRange.IndexOneOctetObjectSize)
                    {
                        //this means range field contains one octet count of objects and
                        //each object is preceded by its index

                    }
                    else if(prefixRange==FormBasedTCPListenOutstation.prefixAndRange.NoIndexOneOctetStartStop)
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
                            for (byte i = startIndex,j=0; i <= stopIndex;i++)
                            {
                                apdu.binaryOutput[startIndex+i] = apduPkt[7+j];
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
            radio1.Checked = (binaryOut[0] > 0)?true:false;
            radio2.Checked = (binaryOut[1] > 0) ? true : false;
            radio3.Checked = (binaryOut[2] > 0) ? true : false;
            
        }

        public void storeOutstationAddr()
        {
            //lets add the ipaddr of this client into our database for future use
            IPAddress newAddr = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
            if (!dnpClientAddr.Contains(newAddr))
            {
                dnpClientAddr.Add(newAddr);
                foreach (IPAddress addr in dnpClientAddr)
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
            Console.Write("processAPDU" + Environment.NewLine);
            byte control = apduPkt[0];
            byte confirmMask = 1 << 5;
            int unsolicitedMask = 1 << 4;
            bool confirmSet = ((control & confirmMask) > 0)?true:false;
            bool unsolicitedSet = ((control & unsolicitedMask) > 0) ? true : false;
            if(confirmSet)
            {
                //do something
            }

            if(unsolicitedSet)
            {
                //do something
            }

            //get function code

          
            FormBasedTCPListenOutstation.functionCode fnCode = (functionCode)apduPkt[1];

            switch(fnCode)
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
                    break; 
                default:
                    break;

            }
        }

        public async Task<List<byte>> processTPDU(List<byte> tpduPkt)
        {
            Console.Write("processTPDU" + Environment.NewLine);
            List<byte> apduExtract = new List<byte>();

            if(tpduPkt.Count > 0)
            {
                Console.WriteLine("correct");
                for(int i=1;i<tpduPkt.Count;i++)
                {
                    apduExtract.Add(tpduPkt[i]);
                }

            }

            return (apduExtract);
        }

        public async Task<List<byte>> processClientMsgAsync(List<byte> msg)
        {
            //Demultiplex the bytes in msg into layers
            //start with extracting the DPDU
            Console.Write("processClientMsgAsync" + Environment.NewLine);
            List<byte> dpduPkt = new List<byte>();
            List<byte> tpduPkt = new List<byte>();
            List<byte> apduPkt = new List<byte>();

            if(msg[0]!=0x05)
            {
                Console.WriteLine("Error not a DNP3 pkt!");
            }
            tpduPkt = await processDPDU(msg);
            apduPkt = await processTPDU(tpduPkt);
            processAPDU(apduPkt);
            return (dpduPkt);
        }

        public async Task<List<byte>> processDPDU(List<byte> dnpPkt)
        {
            Console.Write("processDPDU" + Environment.NewLine);
            //Check the start bytes first
            List<byte> tpduExtract = new List<byte>();

            if(dnpPkt[0] == 0x05)
            {
                if(dnpPkt[1] ==0x64)
                {
                    //Check control byte function code
                    int controlByte = dnpPkt[2];
                    int fnCodeMask = 0x0000000F;
                    int fnCode = controlByte & fnCodeMask; //last 4 bits
                    if((DPDU.functionCode)fnCode == DPDU.functionCode.ISP)
                    {
                        ispPkt = true;
                    }
                    int userDatalength = dnpPkt[2] ;
                    userDatalength -= 5; //reduce 5 for header,i.e. CTRL(1)+DST(2)+SRC(2)

                    //Now calculate the header CRC, the length excludes the 2 CRC bytes at the very end
                    //Calculate header CRC
                    byte[] hdrCRC = dpdu.makeCRC(ref dnpPkt, 0, 7);

                    if(dnpPkt[8]==hdrCRC[0] && dnpPkt[9]==hdrCRC[1])
                    {
                        //Now we can proceed to the Block CRC
                        //We are limiting ourselves to <= 16 bytes but it could be greater so check it
                        //first block starts at dnpPkt[10]

                        if (userDatalength > 16)
                        {
                            byte lenBlock2 = (byte)(userDatalength - (byte)16);
                            //we need two blocks each with its own CRC bytes
                            byte[] crcBlock1 = dpdu.makeCRC(ref dnpPkt, 10, ((10 + 16) - 1));
                            byte[] crcBlock2 = dpdu.makeCRC(ref dnpPkt, (10 + 16), (dnpPkt.Count() - 1)); 

                        }
                        else
                        {
                            byte[] crcBlock = dpdu.makeCRC(ref dnpPkt, 10, (dnpPkt.Count() - 3));

                            int crcIndex1 = dnpPkt.Count - 2;
                            int crcIndex2 = dnpPkt.Count -1;

                            if(crcBlock[0]==dnpPkt[crcIndex1] && crcBlock[1]==dnpPkt[crcIndex2])
                            {
                                //we can now proceed with processing the data link function code
                                Console.WriteLine("Datalink CRC correct!");
                                //now we can extract the TPDU from the DPDU 

                                for(int i=0;i<(int)userDatalength;i++)
                                {
                                    tpduExtract.Add(dnpPkt[i+10]);
                                }


                                
                            }
                        }


                    }
                }
            }

            return (tpduExtract);
        } 

         

        public async void Run(TextBox txtBx, RadioButton rb1, RadioButton rb2, RadioButton rb3)
        {
            radio1 = rb1;
            radio2 = rb2;
            radio3 = rb3;
            stationConsole = txtBx;
            CancellationToken ct;
            List<byte> clientMsg = new List<byte>();
            
            IPAddress self = IPAddress.Parse("192.168.1.200"); 
            // we are listening to all Master devices on port 30000
            TcpListener listener = new TcpListener(IPAddress.Any, 20000);
            listener.Start(); 
           
            //Console.WriteLine(" on port " + 20000 + Environment.NewLine);
            while (true)
            {
                try
                { 
                    Console.WriteLine("Run");
                    tcpClient = await listener.AcceptTcpClientAsync();

                    txtBx.Text += "Received connection" + Environment.NewLine;
                    
                     clientMsg = await readFromClientAsync(tcpClient, ct, txtBx);
                    //we got a message from a client, now we process it
                    
                    await processClientMsgAsync(clientMsg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
            }

                
        }

    

          
            
  }
}

