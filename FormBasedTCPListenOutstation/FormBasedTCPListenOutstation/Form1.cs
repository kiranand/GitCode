using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormBasedTCPListenOutstation
{
   
    public partial class Form1 : Form
    {
        TCPListen service;

        public Form1()
        {

            service = new TCPListen();
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string msgRcvd = await service.Run();
                textBox1.Text += msgRcvd;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }
        }
    }
}
