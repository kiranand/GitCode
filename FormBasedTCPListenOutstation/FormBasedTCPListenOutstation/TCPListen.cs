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
 

namespace FormBasedTCPListenOutstation
{
   
    class TCPListen
    {
        TcpClient tcpClient = new TcpClient();

        RadioButton radio1, radio2, radio3;
        string msgFromClient = "No Data Yet";
        int amountRead = 0;
        int amountWritten = 0;
        bool dataRcvd = false;
        byte[] dataRead = new byte[100];
        DPDU dpdu = new DPDU();
        TPDU tpdu = new TPDU();
        APDU apdu = new APDU();

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
                    
                tcpClient.Close(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected) 
                    tcpClient.Close(); 
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
                    tcpClient.Close();
                return (clientMsg); 
            }
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

                dataServer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close(); 
            }
        }

        
        public async void processAPDUWrite(List<byte> apduPkt)
        {
            //We know this is a write, we need to find out what the indices are and values
            //we also need to know the Group and Variation to perform the action
            //we need to know the qualifier depending on this we make sense of the range values
            
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

        public async void processAPDU(List<byte> apduPkt)
        {
            //process application layer packet
            //CTRL-FNCODE-GRP-VAR-QUAL-RANGE

            //process the CTRL byte which is FIR:FIN:CON:UNS:SEQ
            //we are only interested in the CON bit since that is the only one we are using in an Outstation
            //SEQ is always zero 

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
                    break;

                case FormBasedTCPListenOutstation.functionCode.SELECT:
                    break;

                case FormBasedTCPListenOutstation.functionCode.WRITE:
                    processAPDUWrite(apduPkt);
                    break;

            }
        }

        public List<byte> processTPDU(List<byte> tpduPkt)
        {
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

            List<byte> dpduPkt = new List<byte>();
            List<byte> tpduPkt = new List<byte>();
            List<byte> apduPkt = new List<byte>();

            if(msg[0]!=0x05)
            {
                Console.WriteLine("Error not a DNP3 pkt!");
            }
            tpduPkt = await processDPDU(msg);
            apduPkt = processTPDU(tpduPkt);
            processAPDU(apduPkt);
            return (dpduPkt);
        }

        public async Task<List<byte>> processDPDU(List<byte> dnpPkt)
        {
            //Check the start bytes first
            List<byte> tpduExtract = new List<byte>();

            if(dnpPkt[0] == 0x05)
            {
                if(dnpPkt[1] ==0x64)
                {
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

                            if(crcBlock[0]==dnpPkt[21] && crcBlock[1]==dnpPkt[22])
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
 
            CancellationToken ct;
            List<byte> clientMsg = new List<byte>();
            IPAddress self = IPAddress.Parse("127.0.0.1"); 
            // we are listening to all Master devices on 192.168.1.136 and port 50000
            TcpListener listener = new TcpListener(IPAddress.Any, 20000);
            listener.Start(); 
                
            Console.Write("Array Min and Avg service is now running");
            Console.WriteLine(" on port " + 50000);
            while (true)
            {
                try
                { 
                    Console.WriteLine("Run");
                    tcpClient = await listener.AcceptTcpClientAsync();
                    //ProcessAsync(tcpClient, ct, txtBx); 
                    
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

