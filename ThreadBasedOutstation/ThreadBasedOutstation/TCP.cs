using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ThreadBasedOutstation
{
    class TCP
    {
        //we need a listen thread that listens for connections and accepts
        //we need a read thread that returns data to another thread that will use it
        //we need a write thread that will take in data to be written to a client/Master
        
        public void listen()
        {
           
            TcpListener listener = new TcpListener(IPAddress.Any, 20000);
            listener.Start();
          

            Console.Write("Array Min and Avg service is now running");
            Console.WriteLine(" on port " + 20000);
            while (true)
            {
                try
                {
                    Console.WriteLine("Run");
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    TcpClient tcpClient = 
                    ProcessAsync(tcpClient, ct);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
            }
        }
    }
}
