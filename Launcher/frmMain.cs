// FrmMain.cs


// This drives program flow.  If not defined, the default is manual map injection
//#define MANAGED_INJECT

#region Using Directives
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
#endregion

namespace Launcher
{
    public partial class frmMain : Form
    {
#if !MANAGED_INJECT
        #region Imports
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Map")]
        public static extern uint Map(IntPtr incoming, int size, int processID);
        #endregion
#endif

        #region Local
        IntPtr pointer = IntPtr.Zero;

        string process = String.Empty;
        string dllPath = String.Empty;
        string tempFile = String.Empty;

        static Properties.Settings Settings = Properties.Settings.Default;

        TemporaryFileCleanup customTempFiles = new TemporaryFileCleanup();
        BackgroundWorker worker = new BackgroundWorker();
        Updater updater;

        byte[] fileAsBytes;
        byte[] dll;

        const string DLL = "Shared.dll";
        #endregion

        #region Properties
        public string DllPath
        {
            get => dllPath;
            set => dllPath = value;
        }

        public byte[] FileAsBytes
        {
            get => fileAsBytes;
            set => fileAsBytes = value;
        }

        public string ProcessName
        {
            get => process;
            set => process = value;
        }
        #endregion

        #region Constructors
        static frmMain()
        {
#if !MANAGED_INJECT
            /*
             * Static constructor added to ensure our resource embedded dll(s) are loaded before this class is instantiated.
             * Otherwise the DllImport attributes on imported functions will be unavailable
             */
            EmbeddedDllClass.ExtractEmbeddedDlls(DLL, Properties.Resources.Shared);
            EmbeddedDllClass.LoadDll(DLL);
#endif
        }

        public frmMain()
        {
            InitializeComponent();
        }
        #endregion

        #region Form Events
        private void frmMain_Load(object sender, EventArgs e)
        {
            Settings.Reload();

            updater = new Updater();

#if !DEBUG
            // Forcing injector to ignore existing process
            Boolean found = false;

            if (!updater.CheckForUpdate())
            {

                // First lets determine that ANY version of GTA is installed
                List<string> clients = buildClientsList();

                if (clients.Count == 0)
                {
                    MessageBox.Show(this, "No GTA Installations Found!", "Notice");

                    if (Application.MessageLoop)
                        Application.Exit();
                }
                else
                {
                    foreach (var client in clients)
                    {
                        cmbClientTypeMain.Items.Add(client);
                        cmbClientType.Items.Add(client);
                    }

                    cmbClientTypeMain.SelectedIndex =
                        cmbClientType.SelectedIndex =
                       Settings.GTAClient == "RETAIL"
                       ? 0
                       : 1;

                    lnkEpsilon.Links.Add(0, lnkEpsilon.Text.Length);

                    found = checkForProcess();

                    if (!found)
                        retrieveBinary();
                }
            }
#endif

#if !DEBUG
            if (found)

            if (Application.MessageLoop)
                Application.Exit();

            Size = new System.Drawing.Size(278, 140);
            MaximumSize = Size;
            MinimumSize = Size;

            adminPanel.Visible = false;
            userPanel.Visible = true;
            Settings.AutoClose = true;
            chkCloseAfterInject.Checked = true;
            userPanel.Dock = DockStyle.Fill;
#else
            Size = new System.Drawing.Size(398, 189);
            MaximumSize = Size;
            MinimumSize = Size;
            cmbClientType.Hide();
            label2.Hide();

            userPanel.Visible = false;
            adminPanel.Visible = true;
            chkCloseAfterInject.Checked = false;
            adminPanel.Dock = DockStyle.Fill;
#endif
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            worker = sender as BackgroundWorker;
            worker.WorkerSupportsCancellation = true;

            e.Result = BackgroundProcessLogicMethod(worker);

            if (worker.CancellationPending)
                e.Cancel = true;
        }

        private Process BackgroundProcessLogicMethod(BackgroundWorker worker)
        {
            Process process = null;

            Thread.Sleep(5000);

            while (true)
            {
                if (Process.GetProcessesByName("GTA5").Length > 0)
                    break;

                Thread.Sleep(1000);
            }

            if (Process.GetProcessesByName("GTA5").Length > 0)
                process = Process.GetProcessesByName("GTA5").FirstOrDefault();

            return process;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show(this, "Injection Aborted", "Notice");

                btnInject.Text = "&Inject";
                btnInject.Enabled = true;
            }
            else if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);

                btnInject.Text = "&Inject";
                btnInject.Enabled = true;
            }
            else
            {
                Thread.Sleep(1000);

                Process process = (Process)e.Result;

                // Give enough time for the process to spawn
                Thread.Sleep(15000);

                if (process == null)
                    MessageBox.Show("Process Not Found");
                else
                {
                    btnInject.Text = "&Cancel";
                    btnInject.Enabled = true;
#if MANAGED_INJECT
                    // Managed Inject
                    InjectionResult result = InjectionResult.ProcessNotFound;

                    result = Injector.GetInstance.Inject(process.ProcessName, tempFile);
                    cleanUp(result);
#else
                    uint success = 0;

                    // Manual Map Inject
                    success = Map(pointer, dll.Length, process.Id);
                    closeUp(success);
#endif
                }
            }
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            updater.StopMonitoring();

            if (btnInject.Text == "&Cancel")
            {
                btnInject.Enabled = false;
                worker.CancelAsync();
            }
            else if (ProcessName.Length > 0)
            {
                // In case we cancelled an injection, the button resets so we need to prevent injecting again.
                if (checkForProcess())
                {
                    btnInject.Enabled = true;
                    btnInject.Text = "&Cancel";
                }
                else
                {
                    btnInject.Enabled = false;

                    if (DllPath.Length > 0 && DllPath.Contains(":\\"))
                        dll = File.ReadAllBytes(DllPath);
                    else if (fileAsBytes != null && fileAsBytes.Length > 0)
                        dll = fileAsBytes;

                    pointer = IntPtr.Zero;

                    if (dll.Length > 0)
                        pointer = Marshal.AllocHGlobal(dll.Length);

                    try
                    {
                        Marshal.Copy(dll, 0, pointer, dll.Length);
                        string[] id = ProcessName.Split(new char[] { '|' });

                        Process process;

                        if (ProcessName.Contains("|"))
                        {
                            process = Process.GetProcessById(int.Parse(id[1].Trim()));

                            if (process == null)
                                MessageBox.Show(this, "Process Not Found", "Process");
                            else
                            {
#if MANAGED_INJECT
                            // Managed Injection
                            InjectionResult result = Injector.GetInstance.Inject(process.ProcessName, tempFile);
                            cleanUp(result);
#else
                                uint success = 0;

                                // Manual Map Injection
                                success = Map(pointer, dll.Length, process.Id);

                                closeUp(success);
#endif
                            }
                        }
                        else
                        {
                            // Launch Game
                            process = Process.Start(ProcessName);

                            Thread.Sleep(1000);

                            // GTA5.exe
                            if (process != null)
                                backgroundWorker1.RunWorkerAsync(process);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Exception");
                    }
                }
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            using (frmProcess form = new frmProcess())
            {
                form.ShowDialog(this);

                if (ProcessName.Length > 0)
                    txtProcess.Text = ProcessName;
            }
        }

        private void btnBrowseDll_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (openFileDialog1.FileName.Length > 0)
            {
                DllPath = txtDllPath.Text = openFileDialog1.FileName;
                txtDllPath.Enabled = true;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            exitAndClose();
        }

        private void cmbClientType_SelectedValueChanged(object sender, EventArgs e)
        {
            Settings.GTAClient = cmbClientType.Text;
            Settings.AutoClose = chkCloseAfterInject.Checked;

            Settings.Save();
            Settings.Reload();

            checkForProcess(true);
        }

        private void lnkEpsilon_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Settings.Host);
        }
        #endregion

        #region Private Methods
        private List<string> buildClientsList()
        {
            List<string> clients = new List<string>();

            try
            {
                RegistryKey key = Registry.LocalMachine;

                // Check for Social Club
                key = key.OpenSubKey(@"SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V\");

                if (key != null)
                    clients.Add("RETAIL");

                key = Registry.LocalMachine;

                // Check for Steam
                key = key.OpenSubKey(@"SOFTWARE\WOW6432Node\Rockstar Games\GTAV\");

                if (key != null)
                    clients.Add("STEAM");

            }
            catch (Exception)
            {
                // Eat the exception
            }

            return clients;
        }

        private void exitAndClose()
        {
            if (Application.MessageLoop)
                Application.Exit();
        }

        private void retrieveBinary()
        {
            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("User-Agent", Settings.UserAgent);
                client.Proxy = null;

                FileAsBytes = null;

                Uri uri = new Uri(Settings.BaseURL + Settings.URL);
                if (uri.Scheme == "file")
                {
                    FileAsBytes = File.ReadAllBytes(uri.LocalPath);
                    txtDllPath.Text = "LOCAL DLL LOADED";
                }
                else
                {
                    FileAsBytes = client.DownloadData(uri.AbsoluteUri);
                    txtDllPath.Text = "REMOTE DLL RECEIVED";
                }

#if MANAGED_INJECT
                tempFile = customTempFiles.Path;

                FileStream stream = new FileStream(tempFile, FileMode.OpenOrCreate);
                stream.Write(FileAsBytes, 0, FileAsBytes.Length);
                stream.Close();
#endif

                if (FileAsBytes.Length > 0)
                {
                    txtDllPath.Enabled = false;
#if DEBUG
                    MessageBox.Show(this, "File Received: " + fileAsBytes.Length + " bytes.  Select target process then inject.", "Success");
#endif
                }
            }
            catch (Exception ex)
            {
                FileAsBytes = null;
                tempFile = null;

                MessageBox.Show(this, ex.Message, "Error Reading File");
            }
        }

        private string getGTALocation()
        {
            string result = null;

            try
            {
                RegistryKey key = Registry.LocalMachine;

                switch (Settings.GTAClient)
                {
                    case "RETAIL":
                        // Check for Social Club
                        key = key.OpenSubKey(@"SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V\");

                        if (key != null)
                            result = key.GetValue("InstallFolder").ToString() + @"\PlayGTAV.exe";

                        break;
                    case "STEAM":
                        // Check for Steam
                        key = key.OpenSubKey(@"SOFTWARE\WOW6432Node\Rockstar Games\GTAV\");

                        if (key != null)
                            result = "steam://rungameid/271590";

                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "GTA Not Found!");
            }

            return result;
        }

        private void closeUp(uint success)
        {
#if DEBUG
            MessageBox.Show(this, success == 0
                ? "Injected!"
                : "Injection Failed!", "Status");
#endif
            // If successful, close injector
            if (success == 0 && Settings.AutoClose)
                if (Application.MessageLoop)
                    Application.Exit();

            btnInject.Text = "&Inject";
            btnInject.Enabled = true;
        }

        // Need to check for both GTA5 or PlayGTA5 processes
        private Process findGTA5()
        {
            Process result = null;
            Process[] processes;

            try
            {
                processes = Process.GetProcessesByName("PlayGTAV");

                if (processes.Length > 0)
                    result = processes[0];
                else
                {
                    processes = Process.GetProcessesByName("GTA5");

                    if (processes.Length > 0)
                        result = processes[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error");
            }

            return result;
        }

        private bool checkForProcess(bool silent = false)
        {
            bool found = false;

            if (findGTA5() != null)
            {
                if (!silent)
                    MessageBox.Show(this, "GTA is already running.  Please close and run the launcher again.", "Notice");

                found = true;
            }

            string gta = getGTALocation();

            if (gta != null)
                txtProcess.Text = ProcessName = getGTALocation();

#if !DEBUG
            btnInject.Enabled = false;
#endif

            return found;
        }

        // Managed Inject
        private void cleanUp(InjectionResult result)
        {
            bool success = result == InjectionResult.Success;

            MessageBox.Show(this, success
                ? "Injected!"
                : "Injection Failed!", "Status");

            // If successful, close injector
            if (success)
                if (Application.MessageLoop)
                    Application.Exit();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
#if MANAGED_INJECT
            customTempFiles.Dispose();
#endif
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }
        #endregion
    }
}