using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Miqi.Net;

namespace Miqi
{
    public partial class MainForm : Form
    {
        private MiqiServer server;

        public MainForm()
        {
            InitializeComponent();
        }

        public void Log(string format, params Object[] args) {
            this.Invoke((MethodInvoker)delegate()
            {
                lstLogs.Items.Add(String.Format(format, args));
            });			        
        }

        private void niTray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                // this.Activate();
            }
            else {
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            server = new MiqiServer(this, 54321);
            server.Start();

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                server.Stop();
            }
            catch (Exception) { }
        }

        private void clearLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstLogs.Items.Clear();
        }
    }
}