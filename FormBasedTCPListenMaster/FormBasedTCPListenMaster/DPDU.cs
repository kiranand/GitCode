using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBasedTCPListenMaster
{
    class DPDU
    {
         //The main job of the data link layer is to add the START, LEN, CTRL and CRC bytes

        public byte start1; //0x05
        public byte start2; //0x64
        public byte length; //(5 octets + length of TPDU [which includes APDU])
        public byte control;
        public byte dst;
        public byte src;
        public byte crc1;
        public byte crc2;

        public void setStart1()
        {
            start1 = 0x05;
        }

        public void setStart2()
        {
            start2 = 0x64;
        }

        public void setDst(byte destination)
        {
            dst = destination;
        }

        public void setSrc(byte source)
        {
            src = source;
        }

        public void setControlDIR(bool dir)
        {
            //set the DIR bit in position 7
            if (dir)
            {
                //value is 1 so OR
                control |= 1 << 7; //shift left 7 times and OR
            }
            else
            {
                control = (byte)(control & ~(1 << 7));
            }
        }

        public void setControlPRM(bool prm)
        {
            //set the PRM bit in position 7
            if (prm)
            {
                //value is 1 so OR
                control |= 1 << 6; //shift left 6 times and OR
            }
            else
            {
                control = (byte)(control & ~(1 << 6));
            }
        }

        public void setControlDFC(bool dfc)
        {
            //set the DFC bit in position 4
            if (dfc)
            {
                //value is 1 so OR
                control |= 1 << 4; //shift left 7 times and OR
            }
            else
            {
                control = (byte)(control & ~(1 << 4));
            }
        }

        public void buildDPDU(ref List<byte> pkt, byte control, UInt16 dst, UInt16 src)
        {
            byte userDatalength = (byte)pkt.Count();
            length = (byte)(userDatalength + (byte)5);
            pkt.Insert(0, 0x05); //insert START fields
            pkt.Insert(1, 0x64);

            //we cannot have more than 16-octets in a single block so we need to determine if our user data
            //can fit in a single block.  The data link header is 5 bytes in length (CTRL(1)+DST(1)+SRC(1))
            //the length field includes header length + no of bytes in user data


            byte[] srcBytes = BitConverter.GetBytes(src);
            byte[] dstBytes = BitConverter.GetBytes(dst);
            pkt.Insert(2, length); //insert LENGTH
            pkt.Insert(3, control);  //insert CONTROL
            pkt.Insert(4, dstBytes[0]); //insert DST
            pkt.Insert(5, dstBytes[1]); //insert DST
            pkt.Insert(6, srcBytes[0]); //insert SRC
            pkt.Insert(7, srcBytes[1]); //insert SRC

            //Calculate header CRC
            byte[] hdrCRC = makeCRC(ref pkt, 0, 7);
            pkt.Insert(8, hdrCRC[0]);
            pkt.Insert(9, hdrCRC[1]);


            if (userDatalength > 16)
            {
                byte lenBlock2 = (byte)(userDatalength - (byte)16);
                //we need two blocks each with its own CRC bytes
                byte[] crcBlock1 = makeCRC(ref pkt, 10, ((10 + 16) - 1));
                byte[] crcBlock2 = makeCRC(ref pkt, (10 + 16), (pkt.Count() - 1));
                pkt.Insert((10 + 16), crcBlock1[0]);
                pkt.Insert((10 + 17), crcBlock1[1]);
                pkt.Add(crcBlock2[0]);
                pkt.Add(crcBlock2[1]);

            }
            else
            {
                byte[] crcBlock = makeCRC(ref pkt, 10, (pkt.Count - 1));
                pkt.Add(crcBlock[0]);
                pkt.Add(crcBlock[1]);

            }
        }

        UInt16[] crcLookUpTable = new UInt16[] {

                                0x0000, 0x365E, 0x6CBC, 0x5AE2, 0xD978, 0xEF26, 0xB5C4, 0x839A,
                                0xFF89, 0xC9D7, 0x9335, 0xA56B, 0x26F1, 0x10AF, 0x4A4D, 0x7C13,
                                0xB26B, 0x8435, 0xDED7, 0xE889, 0x6B13, 0x5D4D, 0x07AF, 0x31F1,
                                0x4DE2, 0x7BBC, 0x215E, 0x1700, 0x949A, 0xA2C4, 0xF826, 0xCE78,
                                0x29AF, 0x1FF1, 0x4513, 0x734D, 0xF0D7, 0xC689, 0x9C6B, 0xAA35,
                                0xD626, 0xE078, 0xBA9A, 0x8CC4, 0x0F5E, 0x3900, 0x63E2, 0x55BC,
                                0x9BC4, 0xAD9A, 0xF778, 0xC126, 0x42BC, 0x74E2, 0x2E00, 0x185E,
                                0x644D, 0x5213, 0x08F1, 0x3EAF, 0xBD35, 0x8B6B, 0xD189, 0xE7D7,
                                0x535E, 0x6500, 0x3FE2, 0x09BC, 0x8A26, 0xBC78, 0xE69A, 0xD0C4,
                                0xACD7, 0x9A89, 0xC06B, 0xF635, 0x75AF, 0x43F1, 0x1913, 0x2F4D,
                                0xE135, 0xD76B, 0x8D89, 0xBBD7, 0x384D, 0x0E13, 0x54F1, 0x62AF,
                                0x1EBC, 0x28E2, 0x7200, 0x445E, 0xC7C4, 0xF19A, 0xAB78, 0x9D26,
                                0x7AF1, 0x4CAF, 0x164D, 0x2013, 0xA389, 0x95D7, 0xCF35, 0xF96B,
                                0x8578, 0xB326, 0xE9C4, 0xDF9A, 0x5C00, 0x6A5E, 0x30BC, 0x06E2,
                                0xC89A, 0xFEC4, 0xA426, 0x9278, 0x11E2, 0x27BC, 0x7D5E, 0x4B00,
                                0x3713, 0x014D, 0x5BAF, 0x6DF1, 0xEE6B, 0xD835, 0x82D7, 0xB489,
                                0xA6BC, 0x90E2, 0xCA00, 0xFC5E, 0x7FC4, 0x499A, 0x1378, 0x2526,
                                0x5935, 0x6F6B, 0x3589, 0x03D7, 0x804D, 0xB613, 0xECF1, 0xDAAF,
                                0x14D7, 0x2289, 0x786B, 0x4E35, 0xCDAF, 0xFBF1, 0xA113, 0x974D,
                                0xEB5E, 0xDD00, 0x87E2, 0xB1BC, 0x3226, 0x0478, 0x5E9A, 0x68C4,
                                0x8F13, 0xB94D, 0xE3AF, 0xD5F1, 0x566B, 0x6035, 0x3AD7, 0x0C89,
                                0x709A, 0x46C4, 0x1C26, 0x2A78, 0xA9E2, 0x9FBC, 0xC55E, 0xF300,
                                0x3D78, 0x0B26, 0x51C4, 0x679A, 0xE400, 0xD25E, 0x88BC, 0xBEE2,
                                0xC2F1, 0xF4AF, 0xAE4D, 0x9813, 0x1B89, 0x2DD7, 0x7735, 0x416B,
                                0xF5E2, 0xC3BC, 0x995E, 0xAF00, 0x2C9A, 0x1AC4, 0x4026, 0x7678,
                                0x0A6B, 0x3C35, 0x66D7, 0x5089, 0xD313, 0xE54D, 0xBFAF, 0x89F1,
                                0x4789, 0x71D7, 0x2B35, 0x1D6B, 0x9EF1, 0xA8AF, 0xF24D, 0xC413,
                                0xB800, 0x8E5E, 0xD4BC, 0xE2E2, 0x6178, 0x5726, 0x0DC4, 0x3B9A,
                                0xDC4D, 0xEA13, 0xB0F1, 0x86AF, 0x0535, 0x336B, 0x6989, 0x5FD7,
                                0x23C4, 0x159A, 0x4F78, 0x7926, 0xFABC, 0xCCE2, 0x9600, 0xA05E,
                                0x6E26, 0x5878, 0x029A, 0x34C4, 0xB75E, 0x8100, 0xDBE2, 0xEDBC,
                                0x91AF, 0xA7F1, 0xFD13, 0xCB4D, 0x48D7, 0x7E89, 0x246B, 0x1235
           };


        public byte[] makeCRC(ref List<byte> dpdu, int start, int end)
        {
            int finalCRC = 0;
            for (int j = start; j <= end; j++)
            {
                computeCRC(dpdu[j], ref finalCRC);
            }
            /*foreach (byte x in dpdu)
            {
                computeCRC(x, ref finalCRC);
            }*/

            finalCRC = ~finalCRC; //the bytes in this need to be reversed in order


            byte[] crcBytes = BitConverter.GetBytes(finalCRC); //crcBytes[0] is first CRC byte and crcBytes[1] is second CRC byte
            return crcBytes;
        }

        public void computeCRC(byte dataOctet, ref int crcAccum)// Pointer to 16-bit crc accumulator
        {
            crcAccum = (crcAccum >> 8) ^ crcLookUpTable[(crcAccum ^ dataOctet) & 0xFF];

        }
    }
    
}
