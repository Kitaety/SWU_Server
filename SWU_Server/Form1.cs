using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SWU_Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SystemThread.Instance.OnEvent += AddText;
        }

        private void AddText(string message)
        {

            richTextBox1.BeginInvoke(new MethodInvoker(delegate
            {
                richTextBox1.AppendText( message+"\n");
                richTextBox1.ScrollToCaret();
            }));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            SystemThread.Instance.LoadSettings();
            SystemThread.Instance.StartListener();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
