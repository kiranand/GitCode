﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace FormBasedTCPListenOutstation
{
    public enum functionCode
    {
        CONFIRM = 0x00,
        READ = 0x01,
        WRITE = 0x02,
        SELECT = 0x03,
        OPERATE = 0x04,
        ISP = 0x22, //Interserver Packet
        RESPONSE = 0x81
    }

    



    public enum groupID
    {
        G1 = 0x01, //Binary Inputs
        G2 = 0x02, //Binary Input Event
        GA = 0x0A, //Control or Report state of binary output
        GB = 0x0B, //Binary output event - status without time
        GC = 0x0C //CROB
    }

    public enum variationID
    {
        V1 = 0x01,
        V2 = 0x02
    }

    public enum prefixAndRange
    {
        NoIndexOneOctetStartStop = 0x00,
        IndexOneOctetObjectSize = 0x17
    }
    
    
    class APDU
    {
        public byte controlByte = 0x00;
        public byte functionCodeByte = 0x00;
        public byte IIN_1 = 0x00;
        public byte IIN_2 = 0x00;
        public byte groupByte = 0x00;
        public byte variationByte = 0x00;
        public byte qualifierByte = 0x00; //Res[7]
        //+ Object Prefix Code[6-5-4]
        //+ Range Specifier Code[3-2-1-0]

        public byte range = 0x00;     // this is used to indicate count of objects if range specifier is '7' in qualifier byte 
        public byte rangeStartIndex = 0x00; //this is used if range qualifier is '0', i.e. 1-octet start/stop indices used
        public byte rangeStopIndex = 0x00;
        public byte[] binaryOutput = new byte[3]; //This is the Outstation Database we use this to hold 3 binary output values
        public byte sequence = 0x00;
        

        public void setControlFIR(bool value)
        {
            //set the FIR bit in position 7
            if (value)
            {
                //value is 1 so OR
                controlByte |= 1 << 7; //shift left 7 times and OR
            }
            else
            {
                controlByte = (byte)(controlByte & ~(1 << 7));
            }
        }

        public void setControlFIN(bool value)
        {
            //set the FIR bit in position 7
            if (value)
            {
                //value is 1 so OR
                controlByte |= 1 << 6; //shift left 7 times and OR
            }
            else
            {
                controlByte = (byte)(controlByte & ~(1 << 6));
            }
        }

        public void setControlCON(bool value)
        {
            //set the FIR bit in position 7
            if (value)
            {
                //value is 1 so OR
                controlByte |= 1 << 5; //shift left 7 times and OR
            }
            else
            {
                controlByte = (byte)(controlByte & ~(1 << 5));
            }
        }

        public void setControlUNS(bool value)
        {
            //set the FIR bit in position 7
            if (value)
            {
                //value is 1 so OR
                controlByte |= 1 << 4; //shift left 7 times and OR
            }
            else
            {
                controlByte = (byte)(controlByte & ~(1 << 4));
            }
        }

        public void setSEQ(byte value)
        {
            //This is the first four bits so we must first check to see if value is > 0x0F and if not 
            //then add it to the control byte

            if (value <= 0x0F)
            {  
                controlByte |= sequence++;
            }
        }

        public void setFunctionCode(functionCode value)
        {
            functionCodeByte = (byte)value;
        }

        public void setIIN_1()
        {
            IIN_1 = 0x00;
        }

        public void setIIN_2()
        {
            IIN_2 = 0x00;
        }

        public void setGroupID(groupID value)
        {
            groupByte = (byte)value;
        }

        public void setVariation(variationID value)
        {
            variationByte = (byte)value;
        }

        public void setQualifier(prefixAndRange val)
        {
            if (val == prefixAndRange.NoIndexOneOctetStartStop)
            {
                qualifierByte = 0x00;
            }
            else if (val == prefixAndRange.IndexOneOctetObjectSize)
            {
                qualifierByte = 0x17;
            }
        }

        public void setRange(byte value)
        {
            range = value;
        }

        public void setRangeStart(byte start)
        {
            rangeStartIndex = start;

        }

        public void setRangeStop(byte stop)
        {
            rangeStopIndex = stop;

        }


        public void buildAPDU(ref List<byte> apdu, params byte[] values)
        {  
            //params should be in the following order
            //confirm, unsolicited, function, group, variation, prefixQualifier, [range] OR [start index, stop index]
            setControlFIR(true);
            setControlFIN(true);
            bool con = (values[0] > 0) ? true : false;
            setControlCON(con);
            bool uns = (values[1] > 0) ? true : false;
            setControlUNS(uns);
            apdu.Add(controlByte);

            setFunctionCode((functionCode)values[2]);
            apdu.Add(functionCodeByte);

            if (functionCodeByte == (byte)functionCode.RESPONSE)
            {
                setIIN_1();
                apdu.Add(IIN_1);
                setIIN_2();
                apdu.Add(IIN_2);

            }
             
            
            setGroupID((groupID)values[3]);
            apdu.Add(groupByte);

            setVariation((variationID)values[4]);
            apdu.Add(variationByte);

            setQualifier((prefixAndRange)values[5]);
            apdu.Add(qualifierByte);
            //now test prefixAndRange to see if its zero or 0x17
            if (values[5] > 0) //means it is 0x17
            {
                //we are expecting a range for the next parameter
                setRange(values[6]); //values[6] would contain an object count or range
                apdu.Add(range);

                //Now get the 'range' count of index-value pairs
                //we need to know if it is a read or write because if it is a write then we have extra bytes of write-value
                if (functionCodeByte == (byte)functionCode.WRITE)
                {
                    //It is a write operation
                    //we need to first make sure that there are 2 x range byte values in the values[] arra
                    //for example if the range is 2 then we need index1-value1, index2-value2 which is 4 bytes
                    int arraySize = values.Length;
                    int indexValueNumbers = arraySize - 7; //since we are at index 6 in the array
                    if (indexValueNumbers != values[6] * 2)
                    {
                        Console.WriteLine("Error! Insufficient index-value information");
                    }
                    else
                    {
                        for (UInt16 count = 0; count < (2 * values[6]); count++)
                        {
                            int index = count + 7;
                            apdu.Add(values[index]);
                        }
                    }
                }
                else if(functionCodeByte == (byte)functionCode.READ)
                {
                    //It is a read operation
                    //we need to first make sure that there are 2 x range byte values in the values[] arra
                    //for example if the range is 2 then we need index1-value1, index2-value2 which is 4 bytes
                    int arraySize = values.Length;
                    int indexValueNumbers = arraySize - 7; //since we are at index 6 in the array
                    if (indexValueNumbers != values[6] * 2)
                    {
                        Console.WriteLine("Error! Insufficient index-value information");
                    }
                    else
                    {
                        for (UInt16 count = 0; count < (2 * values[6]); count++)
                        {
                            int index = count + 7;
                            apdu.Add(values[index]);
                        }
                    } 

                }
            }
            else if (values[5].Equals(0))
            {
                setRangeStart(values[6]);
                apdu.Add(values[6]);

                setRangeStop(values[7]);
                apdu.Add(values[7]);

                if (functionCodeByte == (byte)functionCode.WRITE)
                {
                    //It is a write operation
                    //we need to first make sure that there are 2 x range byte values in the values[] arra
                    //for example if the range is 2 then we need index1-value1, index2-value2 which is 4 bytes
                    int arraySize = values.Length;
                    int indexValueNumbers = arraySize - 8; //since we are at index 7 in the array
                    int valuesToWrite = (values[7] - values[6]) + 1;
                    if (indexValueNumbers != valuesToWrite)
                    {
                        Console.WriteLine("Error! Insufficient index-value information");
                    }
                    else
                    {
                        for (UInt16 count = 0; count < valuesToWrite; count++)
                        {
                            int index = count + 8;
                            apdu.Add(values[index]);
                        }
                    }
                }
                else if(functionCodeByte == (byte)functionCode.RESPONSE)
                {
                    //We are building a DNP response to a read cmd
                    int valuesToWrite = (values[7] - values[6]) + 1;
                    byte[] binaryResponse = new byte[1];
                    var bitArray = new BitArray(binaryResponse);
                  
                    for (UInt16 count = 0; count < valuesToWrite; count++)
                    {
                        //we are only dealing with binary inputs so we need to pack responses as bit values
                        //since we only have 3 binary inputs we only set 3 values in one response byte
                        //examine values and set bits accordingly based on position
                       
                        int index = count + 8;

                        if (values[index]==1)
                        {
                            bitArray.Set(count, true);
                        }
                        //apdu.Add(values[index]);
                    } 

                    bitArray.CopyTo(binaryResponse, 0);  
                    apdu.Add(binaryResponse[0]);
                }
                else if (functionCodeByte == (byte)functionCode.ISP)
                {
                    //It is a ISP operation
                    //we need to first make sure that there are 12 bytes of IP addr + client HW addr after the range byte
                    int arraySize = values.Length;
                    int indexValueNumbers = arraySize - 8; //since we are at index 7 in the array
                    int valuesToWrite = (values[7] - values[6]) + 1;
                    if (indexValueNumbers != valuesToWrite)
                    {
                        Console.WriteLine("Error! Insufficient index-value information");
                    }
                    else
                    {
                        for (UInt16 count = 0; count < valuesToWrite; count++)
                        {
                            int index = count + 8;
                            apdu.Add(values[index]);
                        }
                    }
                }


            }
            else
            {
                Console.WriteLine("Invalid prefixAndRange value");
            }


        }


      
    }


   






}
