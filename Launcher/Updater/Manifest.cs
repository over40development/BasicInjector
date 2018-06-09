#region Using Directives
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
#endregion

namespace Launcher
{
    /// <summary>
    /// Update Manifest
    /// </summary>
    internal class Manifest
    {
        #region Fields
        private string _data;
        private static Properties.Settings Settings = Properties.Settings.Default;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Manifest"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Manifest(string data)
        {
            Settings.Reload();

            Load(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Manifest"/> class.
        /// </summary>
        public Manifest()
        {
            Settings.Reload();

            Load(Properties.Resources.Update);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public int Version { get; private set; }

        /// <summary>
        /// Gets the check interval.
        /// </summary>
        /// <value>The check interval.</value>
        public int CheckInterval { get; private set; }

        /// <summary>
        /// Gets the remote configuration URI.
        /// </summary>
        /// <value>The remote configuration URI.</value>
        public string RemoteConfigUri { get; private set; }

        /// <summary>
        /// Gets the security token.
        /// </summary>
        /// <value>The security token.</value>
        public string SecurityToken { get; private set; }

        /// <summary>
        /// Gets the base URI.
        /// </summary>
        /// <value>The base URI.</value>
        public string BaseUri { get; private set; }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>The payload.</value>
        public string[] Payloads { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Loads the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        private void Load(string data)
        {
            _data = data;
            try
            {
                // Load config from XML
                var xml = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(_data)));
                if (xml.Root.Name.LocalName != "Manifest")
                {
                    Console.WriteLine("Root XML element '{0}' is not recognized, stopping.", xml.Root.Name);
                    return;
                }

                // Set properties.
                Version = int.Parse(xml.Root.Attribute("version").Value);
                CheckInterval = int.Parse(xml.Root.Element("CheckInterval").Value);
                SecurityToken = xml.Root.Element("SecurityToken").Value;
                RemoteConfigUri = xml.Root.Element("RemoteConfigUri").Value;
                BaseUri = xml.Root.Element("BaseUri").Value;
                Payloads = xml.Root.Elements("Payload").Select(x => x.Value).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                return;
            }
        }

        /// <summary>
        /// Writes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Write(string path)
        {
            File.WriteAllText(path, _data);
        }

        /// <summary>
        /// Update embeded resource
        /// </summary>
        /// <returns></returns>
        public bool UpdateResourceXMLFile(int version)
        {
            bool success = false;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(new MemoryStream(Encoding.UTF8.GetBytes(_data)));

                Settings.ManifestVersion = version;
                Settings.Save();
                Settings.Reload();

                success = true;
            } catch (Exception)
            {
                // Eat exception
            }

            return success;
        }

        /// <summary>
        /// Retrieve embeded resource
        /// </summary>
        /// <returns></returns>
        public string GetResourceXMLFile()
        {
            return Settings.Manifest.OuterXml;
        }
        #endregion
    }
}
