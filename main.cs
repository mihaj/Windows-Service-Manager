using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WindowsServiceDashboard
{
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
        }

        string serviceName = "";
        string serviceEXE = "";

        public static void StartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        public static void StopService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void RestartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            StartService(serviceName, 15000);
            ReloadButtons();
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            StopService(serviceName, 15000);
            ReloadButtons();
        }

        private void btn_restart_Click(object sender, EventArgs e)
        {
            StopService(serviceName, 15000);
            ReloadButtons();
            StartService(serviceName, 15000);
            ReloadButtons();
        }

        private void btn_install_Click(object sender, EventArgs e)
        {
            
            var serviceExists = ServiceController.GetServices().Any(s => s.ServiceName == serviceName);
            if (serviceExists)
            {
                MessageBox.Show("Service already installed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(@"c:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe"))
            {
                MessageBox.Show("InstallUtil.exe does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + serviceEXE))
            {
                MessageBox.Show(serviceEXE + " does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Process proc = new Process();
                proc.StartInfo = new ProcessStartInfo(@"c:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe", "\"" + AppDomain.CurrentDomain.BaseDirectory + serviceEXE + "\"");
                proc.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Thread.Sleep(1000);
            ReloadButtons();
        }

        private void btn_uninstall_Click(object sender, EventArgs e)
        {
            var serviceExists = ServiceController.GetServices().Any(s => s.ServiceName == serviceName);
            if (!serviceExists)
            {
                MessageBox.Show("Service not installed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(@"c:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe"))
            {
                MessageBox.Show("InstallUtil.exe does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + serviceEXE))
            {
                MessageBox.Show(serviceEXE + " does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            try
            {
                Process proc = new Process();
                proc.StartInfo = new ProcessStartInfo(@"c:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe", "/u \"" + AppDomain.CurrentDomain.BaseDirectory + serviceEXE + "\"");
                proc.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Thread.Sleep(1000);
            ReloadButtons();
        }

        private void ReloadButtons()
        {
            var serviceExists = ServiceController.GetServices().Any(s => s.ServiceName == serviceName);

            if (serviceExists == true)
            {
                ServiceController service = new ServiceController(serviceName);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    btn_uninstall.Enabled = false;
                    btn_stop.Enabled = true;
                    btn_start.Enabled = false;
                    btn_install.Enabled = false;
                    btn_restart.Enabled = true;

                    status.Text = "Service is running...";
                }
                else
                {
                    btn_uninstall.Enabled = true;
                    btn_stop.Enabled = false;
                    btn_start.Enabled = true;
                    btn_install.Enabled = false;
                    btn_restart.Enabled = false;

                    status.Text = "Service is " + service.Status.ToString() + ".";
                }
            }
            else
            {
                btn_uninstall.Enabled = false;
                btn_stop.Enabled = false;
                btn_start.Enabled = false;
                btn_install.Enabled = true;
                btn_restart.Enabled = false;

                status.Text = "Service not installed.";
            }
        }

        private void main_Load(object sender, EventArgs e)
        {
            serviceName = Properties.Settings.Default.ServiceName;
            serviceEXE = Properties.Settings.Default.ServiceName + ".exe";

            serviceList.Items.Add(Properties.Settings.Default.ServiceName);

            serviceList.SelectedIndex = 0;

            ReloadButtons();
        }

        private void serviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + serviceEXE))
            {
                if (MessageBox.Show(serviceEXE + " does not exist. Closing...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                {
                    Application.Exit();
                }
            }

            ReloadButtons();
        }


        private bool KillProcessByID(string serviceExe, bool waitForExit = false)
        {
            int procID = 0;
            Process[] procs = Process.GetProcessesByName(serviceExe);

            Process proc = null;

            if (procs.Length > 0)
            {
                proc = procs[0];
            }
            else
            {
                return false;
            }

            if (proc != null)
            {
                procID = proc.Id;
            }
            else
            {
                return false;
            }

            if (procID > 0)
            {
                using (Process p = Process.GetProcessById(procID))
                {
                    if (p == null || p.HasExited) return false;

                    p.Kill();
                    if (waitForExit)
                    {
                        p.WaitForExit();
                    }
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private void btn_kill_Click(object sender, EventArgs e)
        {
            if (KillProcessByID(serviceName))
            {
                MessageBox.Show(serviceName + " was terminated.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(serviceName + " was not terminated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
