using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Diagnostics;

namespace FormBasedTCPListenOutstation
{
    public static class NetworkConfigurator
    {
        /// <summary>
        /// Returns the network card configuration of the specified NIC
        /// </summary>
        /// <PARAM name="nicName">Name of the NIC</PARAM>
        /// <PARAM name="ipAdresses">Array of IP</PARAM>
        /// <PARAM name="subnets">Array of subnet masks</PARAM>
        /// <PARAM name="gateways">Array of gateways</PARAM>
        /// <PARAM name="dnses">Array of DNS IP</PARAM>
        public static void GetIP(string nicName, out string[] ipAdresses,
          out string[] subnets, out string[] gateways, out string[] dnses)
        {
            ipAdresses = null;
            subnets = null;
            gateways = null;
            dnses = null;

            ManagementClass mc = new ManagementClass(
              "Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                // Make sure this is a IP enabled device. 
                // Not something like memory card or VM Ware
                if (mo["ipEnabled"] is bool)
                {
                    if (mo["Caption"].Equals(nicName))
                    {
                        ipAdresses = (string[])mo["IPAddress"];
                        subnets = (string[])mo["IPSubnet"];
                        gateways = (string[])mo["DefaultIPGateway"];
                        dnses = (string[])mo["DNSServerSearchOrder"];

                        break;
                    }
                }
            }
        }




        public static void setIP(string _iP, string subnetMask, string _defaultGateway)
        {

            string cmd = "";

            cmd = "interface ip set address name = \"Ethernet\" static " + _iP + " " + subnetMask + " " + _defaultGateway;

            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo("netsh", cmd);
            p.StartInfo = psi;
            psi.Verb = "runas";
            p.Start();
            System.Threading.Thread.Sleep(20000);
        }
    }
}

