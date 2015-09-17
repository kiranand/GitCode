using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace FormBasedTCPListenOutstation
{
    public delegate void workerFunctionDelegate(byte[] msg);

    public partial class Form1 : Form
    {
        TCPListen service;

        public Form1()
        {

            service = new TCPListen();
            InitializeComponent();
        }

        private  void button1_Click(object sender, EventArgs e)
        {
            try
            {
                 service.Run(textBox1);
                Console.Write("running GUI");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }
        }
    }
}
