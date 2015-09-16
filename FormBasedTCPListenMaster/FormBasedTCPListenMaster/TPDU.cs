using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBasedTCPListenMaster
{
    class TPDU
    {

        //  The transport layer only adds a one byte header to the APDU and sends it to the Data Link Layer 

        public void buildTPDU(ref List<byte> apdu)
        {
            byte header = 0xC0;  //FIN=1, FIR=1, SEQ=0
            apdu.Insert(0, header); //insert at top of packet
        }
    }
}
