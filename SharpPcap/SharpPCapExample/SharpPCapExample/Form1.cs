using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.WinPcap;
using SharpPcap.AirPcap;
using PacketDotNet;

namespace SharpPCapExample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Print SharpPcap version 
            string ver = SharpPcap.Version.VersionString;
            Console.WriteLine("SharpPcap {0}, Example1.IfList.cs", ver);

            // Retrieve the device list
            SharpPcap.CaptureDeviceList devices;
            devices = SharpPcap.CaptureDeviceList.Instance;

            // If no devices were found print an error
            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine");
                return;
            }

            //Console.WriteLine("\nThe following devices are available on this machine:");
            //Console.WriteLine("----------------------------------------------------\n");

            // Print out the available network devices
            //foreach (ICaptureDevice dev in devices)
            //    Console.WriteLine("{0}\n", dev.ToString());

            //Build pkt

            byte[] sendData = { 0xDE, 0xAD, 0xBE, 0xEF };
            string dataString = "HELP!";
            ushort tcpSourcePort = 20000;
            ushort tcpDestinationPort = 20000; 
            var tcpPacket = new TcpPacket(tcpSourcePort, tcpDestinationPort);
            tcpPacket.PayloadData = sendData; 
            var ipSourceAddress = System.Net.IPAddress.Parse("192.168.1.225");
            var ipDestinationAddress = System.Net.IPAddress.Parse("192.168.1.91");
            var ipPacket = new IPv4Packet(ipSourceAddress, ipDestinationAddress);

            var sourceHwAddress = "78-E3-B5-57-BC-90";
            var ethernetSourceHwAddress = System.Net.NetworkInformation.PhysicalAddress.Parse(sourceHwAddress);
            var destinationHwAddress = "00-25-64-EC-71-FF";
            var ethernetDestinationHwAddress = System.Net.NetworkInformation.PhysicalAddress.Parse(destinationHwAddress);

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
            for (ushort i = 0; i < 3; i++)
            {
                 device = devices[i];
                device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
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
             

            // Close the pcap device
            
            Console.WriteLine("-- Device closed.");
        }
    }
}
 
