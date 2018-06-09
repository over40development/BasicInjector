// TemporaryFileCleanup.cs


#region Directives
using System;
#endregion

namespace Launcher
{
    /// <summary>
    /// Temporary .NET file cleanup
    /// </summary>
    public class TemporaryFileCleanup : IDisposable
    {
        private string path;

        /// <summary>
        /// Path Property
        /// </summary>
        public string Path
        {
            get
            {
                return path;
            }
        }

        /// <summary>
        /// Temporary File Cleanup
        /// </summary>
        public TemporaryFileCleanup()
        {
            path = System.IO.Path.GetTempFileName();
        }

        /// <summary>
        /// Disposable Resource Cleanup
        /// </summary>
        public void Dispose()
        {
            try
            {
                System.IO.File.Delete(path);
            }
            catch (System.IO.IOException)
            {
                // Whoops
            }
        }
    }
}
