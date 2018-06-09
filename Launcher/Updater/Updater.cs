#region Using Directives
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
#endregion

namespace Launcher
{
    /// <summary>
    /// In-place auto update class
    /// </summary>
    public class Updater
    {
        #region Constants
        /// <summary>
        /// The default check interval
        /// </summary>
        public const int DefaultCheckInterval = 900;
        public const int FirstCheckDelay = 15;

        /// <summary>
        /// The default configuration file
        /// </summary>
        public const string DefaultConfigFile = "Update.xml";

        /// <summary>
        /// Temporary work path
        /// </summary>
        public const string WorkPath = "_temp";
        #endregion

        #region Fields
        private System.Threading.Timer _timer;
        private volatile bool _updating;
        private readonly Manifest _localConfig;
        private Manifest _remoteConfig;
        private readonly FileInfo _localConfigFile;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Updater"/> class.
        /// </summary>
        public Updater()
        {
            _localConfig = new Manifest();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Updater"/> class.
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        public Updater(FileInfo configFile)
        {
            _localConfigFile = configFile;
            Console.WriteLine("Loaded.");
            Console.WriteLine("Initializing using file '{0}'.", configFile.FullName);

            if (!configFile.Exists)
            {
                Console.WriteLine("Config file '{0}' does not exist, stopping.", configFile.Name);
                return;
            }

            _localConfig = new Manifest();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the monitoring.
        /// </summary>
        public void StartMonitoring()
        {
            Console.WriteLine("Starting monitoring every {0}s.", _localConfig.CheckInterval);
            _timer = new System.Threading.Timer(Check, null, 5000, _localConfig.CheckInterval * 1000);
        }

        /// <summary>
        /// Stops the monitoring.
        /// </summary>
        public void StopMonitoring()
        {
            Console.WriteLine("Stopping monitoring.");
            if (_timer == null)
            {
                Console.WriteLine("Monitoring was already stopped.");
                return;
            }

            _timer.Dispose();
        }

        public bool CheckForUpdate()
        {
            try
            {
                Console.WriteLine("Check starting.");

                var remoteUri = new Uri(_localConfig.RemoteConfigUri);

                Console.WriteLine("Fetching '{0}'.", remoteUri.AbsoluteUri);
                var http = new Fetch
                {
                    Retries = 5,
                    RetrySleep = 30000,
                    Timeout = 30000
                };

                http.Load(remoteUri.AbsoluteUri);
                if (!http.Success)
                {
                    Console.WriteLine("Fetch error: {0}", http.Response.StatusDescription);
                    _remoteConfig = null;
                    return false;
                }

                string data = Encoding.UTF8.GetString(http.ResponseData);
                _remoteConfig = new Manifest(data);

                if (_remoteConfig == null)
                    return false;

                if (_localConfig.SecurityToken != _remoteConfig.SecurityToken)
                {
                    Console.WriteLine("Security token mismatch.");
                    return false;
                }

                Console.WriteLine("Remote config is valid.");
                Console.WriteLine("Local version is  {0}.", _localConfig.Version);
                Console.WriteLine("Remote version is {0}.", _remoteConfig.Version);

                if (_remoteConfig.Version == _localConfig.Version)
                {
                    Console.WriteLine("Versions are the same.");
                    Console.WriteLine("Check ending.");
                    return false;
                }

                if (_remoteConfig.Version < _localConfig.Version)
                {
                    Console.WriteLine("Remote version is older.");
                    Console.WriteLine("Check ending.");
                    return false;
                }

                Console.WriteLine("Remote version is newer. Updating.");
                _updating = true;

                switch (MessageBox.Show("Preparing to download update.", "New Update Available"))
                {
                    case DialogResult.OK:
                    default:
                        Update();
                        break;
                }

                _updating = false;
                Console.WriteLine("Check ending.");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Error");

                return false;
            }
        }

        /// <summary>
        /// Checks the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        private void Check(object state)
        {
            try
            {
                Console.WriteLine("Check starting.");

                if (_updating)
                {
                    Console.WriteLine("Updater is already updating.");
                    Console.WriteLine("Check ending.");
                }

                var remoteUri = new Uri(_localConfig.RemoteConfigUri);

                Console.WriteLine("Fetching '{0}'.", remoteUri.AbsoluteUri);
                var http = new Fetch {
                    Retries = 5,
                    RetrySleep = 30000,
                    Timeout = 30000
                };

                http.Load(remoteUri.AbsoluteUri);
                if (!http.Success)
                {
                    Console.WriteLine("Fetch error: {0}", http.Response.StatusDescription);
                    _remoteConfig = null;
                    return;
                }

                string data = Encoding.UTF8.GetString(http.ResponseData);
                _remoteConfig = new Manifest(data);

                if (_remoteConfig == null)
                    return;

                if (_localConfig.SecurityToken != _remoteConfig.SecurityToken)
                {
                    Console.WriteLine("Security token mismatch.");
                    return;
                }

                Console.WriteLine("Remote config is valid.");
                Console.WriteLine("Local version is  {0}.", _localConfig.Version);
                Console.WriteLine("Remote version is {0}.", _remoteConfig.Version);

                if (_remoteConfig.Version == _localConfig.Version)
                {
                    Console.WriteLine("Versions are the same.");
                    Console.WriteLine("Check ending.");
                    return;
                }

                if (_remoteConfig.Version < _localConfig.Version)
                {
                    Console.WriteLine("Remote version is older.");
                    Console.WriteLine("Check ending.");
                    return;
                }

                Console.WriteLine("Remote version is newer. Updating.");
                _updating = true;

                switch (MessageBox.Show("Preparing to download update.", "New Update Available"))
                {
                    case DialogResult.OK:
                    default:
                        Update();
                        break;
                }

                _updating = false;
                Console.WriteLine("Check ending.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Error");
            }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        private void Update()
        {
            try
            {
                Console.WriteLine("Updating '{0}' files.", _remoteConfig.Payloads.Length);

                // Clean up failed attempts.
                if (Directory.Exists(WorkPath))
                {
                    Console.WriteLine("WARNING: Work directory already exists.");

                    try
                    {
                        Directory.Delete(WorkPath, true);
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Cannot delete open directory '{0}'.", WorkPath);
                        return;
                    }
                }

                Directory.CreateDirectory(WorkPath);

                // Download files in manifest.
                foreach (string update in _remoteConfig.Payloads)
                {
                    Console.WriteLine("Fetching '{0}'.", update);
                    var url = _remoteConfig.BaseUri + update;
                    var file = Fetch.Get(url);

                    if (file == null)
                    {
                        Console.WriteLine("Fetch failed.");
                        return;
                    }

                    var info = new FileInfo(Path.Combine(WorkPath, update));
                    Directory.CreateDirectory(info.DirectoryName);
                    File.WriteAllBytes(Path.Combine(WorkPath, update), file);

                    // Unzip
                    if (Regex.IsMatch(update, @"\.zip"))
                    {
                        try
                        {
                            string zipfile = Path.Combine(WorkPath, update);

                            using (ZipArchive archive = ZipFile.OpenRead(zipfile))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    if (entry.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                        entry.ExtractToFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + WorkPath, entry.FullName), true);
                                }
                            }

                            if (File.Exists(zipfile))
                                File.Delete(zipfile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Unpack failed: {0}", ex.Message);
                            return;
                        }
                    }
                }

                // Change the currently running executable so it can be overwritten.
                Process process = Process.GetCurrentProcess();
                string me = process.MainModule.FileName;
                string bak = me + ".bak";
                Console.WriteLine("Renaming running process to '{0}'.", bak);

                if (File.Exists(bak))
                    File.Delete(bak);

                File.Move(me, bak);
                File.Copy(bak, me);

                // Update new manifest.
                if (!_remoteConfig.UpdateResourceXMLFile(_remoteConfig.Version))
                    return;

                // Copy everything.
                var directory = new DirectoryInfo(WorkPath);
                var files = directory.GetFiles("*.*", SearchOption.AllDirectories);

                foreach (FileInfo file in files)
                {
                    string destination = file.FullName.Replace(directory.FullName + @"\", "");
                    Console.WriteLine("installing file '{0}'.", destination);
                    Directory.CreateDirectory(new FileInfo(destination).DirectoryName);
                    file.CopyTo(destination, true);
                }

                // Clean up.
                Console.WriteLine("Deleting work directory.");
                Directory.Delete(WorkPath, true);

                MessageBox.Show("Update Successful.  Restarting!", "Success");

                // Restart.
                Console.WriteLine("Spawning new process.");

                var spawn = Process.Start(me);
                Console.WriteLine("New process ID is {0}", spawn.Id);
                Console.WriteLine("Closing old running process {0}.", process.Id);

                process.CloseMainWindow();
                process.Close();
                process.Dispose();

                if (File.Exists(bak))
                    File.Delete(bak);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Error");
            }
        }
        #endregion
    }
}