using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
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

        IPAddress localIPAddr;
        PhysicalAddress etherAddress;

        public Form1()
        {
            InitializeComponent();
        }

        

        public void setLocalAddr(TextBox addresses)
        {
            //first get out IP address and store it for later use
            string str = "";
            stationConsole.Text += "Determine Local Addr" + Environment.NewLine;
            System.Net.NetworkInformation.NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            List<string> ipAddr = new List<string>();
            SharpPcap.CaptureDeviceList devices;

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

                    etherAddress = adapter.GetPhysicalAddress();
                    byte[] bytes = etherAddress.GetAddressBytes();
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
                    localIPAddr = IPAddress.Parse(ipAddr[i]);
                    addresses.Text += localIPAddr;
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

        private void buttonInit_Click(object sender, EventArgs e)
        {
            setLocalAddr(stationConsole);
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
            var ipDestinationAddress = System.Net.IPAddress.Parse(textBoxIPAddr.Text);
            var ipPacket = new IPv4Packet(localIPAddr, ipDestinationAddress); 
            var sourceHwAddress = textBoxEtherAddr;
            var ethernetSourceHwAddress = etherAddress; 
            var ethernetDestinationHwAddress = System.Net.NetworkInformation.PhysicalAddress.Parse(textBoxEtherAddr.Text);

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
            //Console.WriteLine(ethernetPacket.ToString());
            stationConsole.Text += ethernetPacket.ToString() + Environment.NewLine;

            // to retrieve the bytes that represent this newly created EthernetPacket use the Bytes property
            byte[] packetBytes = ethernetPacket.Bytes;

            // Open the device
            // Extract a device from the list
            

            // Open the device for capturing
            int readTimeoutMilliseconds = 1000;
            ICaptureDevice device;
            for (ushort i = 0; i < devices.Count(); i++)
            {
                 device = devices[i];
                device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
                try
                {
                    // Send the packet out the network device
                    device.SendPacket(packetBytes);
                    stationConsole.Text += "-- Packet sent successfuly." + Environment.NewLine;
                    //Console.WriteLine("-- Packet sent successfuly.");
                    device.Close();
                }
                catch (Exception eX)
                {
                    //Console.Write("Pkt send failed" + Environment.NewLine);
                    stationConsole.Text += "Pkt send failed" + Environment.NewLine;
                }
            }
             

            // Close the pcap device
            
            Console.WriteLine("-- Device closed.");
        }

        private void stationConsole_TextChanged(object sender, EventArgs e)
        { 
            stationConsole.SelectionStart = stationConsole.Text.Length;
            stationConsole.ScrollToCaret(); 
        }

        
    }
}
 
