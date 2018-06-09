// EmbeddedDllClass.cs


#region Using Directives
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
#endregion

namespace Launcher
{
    /// <summary>
    /// Embed DLL as resource
    /// </summary>
    public class EmbeddedDllClass
    {
        #region P/Invoke
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);
        #endregion

        private static string tempFolder = "";

        /// <summary>
        /// Extract DLLs from resources to temporary folder
        /// </summary>
        /// <param name="dllName">name of DLL file to create (including dll suffix)</param>
        /// <param name="resourceBytes">The resource name (fully qualified)</param>
        public static void ExtractEmbeddedDlls(string dllName, byte[] resourceBytes)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            AssemblyName assemblyName = assembly.GetName();

            // The temporary folder holds one or more of the temporary DLLs; made "unique" to avoid different versions of the DLL or architectures.
            tempFolder = String.Format("{0}.{1}.{2}", assemblyName.Name, assemblyName.ProcessorArchitecture, assemblyName.Version);

            string directory = Path.Combine(Path.GetTempPath(), tempFolder);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Add the temporary directory to the PATH environment variable (at the head!)
            string path = Environment.GetEnvironmentVariable("PATH");
            string[] parts = path.Split(';');
            bool found = false;

            foreach (string pathPiece in parts)
            {
                if (pathPiece == directory)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                Environment.SetEnvironmentVariable("PATH", directory + ";" + path);

            // See if the file exists, avoid rewriting it if not necessary
            string dllPath = Path.Combine(directory, dllName);
            bool rewrite = true;

            if (File.Exists(dllPath))
            {
                byte[] existing = File.ReadAllBytes(dllPath);
                if (resourceBytes.SequenceEqual(existing))
                    rewrite = false;
            }

            if (rewrite)
                File.WriteAllBytes(dllPath, resourceBytes);
        }

        /// <summary>
        /// Managed wrapper around LoadLibrary
        /// </summary>
        /// <param name="dllName"></param>
        static public void LoadDll(string dllName)
        {
            if (tempFolder == "")
                throw new Exception("Please call ExtractEmbeddedDlls before LoadDll");
            
            IntPtr h = LoadLibrary(dllName);

            if (h == IntPtr.Zero)
            {
                Exception e = new Win32Exception();

                throw new DllNotFoundException("Unable to load library: " + dllName + " from " + tempFolder, e);
            }
        }
    }
}