using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZTS.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void registerTurnstileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegisterTurnstile newform = new RegisterTurnstile();
            newform.Show();
        }

        private void registerWatchServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegisterWatchServer newform = new RegisterWatchServer();
            newform.Show();
        }

        private void turnstileControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ////string s = Application.StartupPath;
            ////string path = Application.StartupPath + "/Wat/ZTS.Watch.exe";
            ////System.Diagnostics.Process.Start(path);
            //ZTS.Watch.WatchTurnstiles newform = new ZTS.Watch.WatchTurnstiles();
            ////WatchTurnstiles newform = new WatchTurnstiles();
            //newform.Show();
        }

        private void functionTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FunctionsTest newform = new FunctionsTest();
            newform.Show(this);
        }
    }
}
