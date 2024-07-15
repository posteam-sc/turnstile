using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Gate5.Watch
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Process aprocess = new Process();
            aprocess = Process.GetCurrentProcess();
            String aprocname = aprocess.ProcessName;

            if (Process.GetProcessesByName(aprocname).Length > 1)
            {
                MessageBox.Show("The application is already running!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WatchTurnstiles());
        }
    }
}
