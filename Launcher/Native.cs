// Native.cs


#region Using Directives
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
#endregion

namespace Launcher
{
    public enum MemoryProtectionConsts : uint
    {
        EXECUTE = 0x10,
        EXECUTE_READ = 0x20,
        EXECUTE_READWRITE = 0x40,
        NOACCESS = 0x01,
        READONLY = 0x02,
        READWRITE = 0x04
    }

    public enum AllocationType
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        Release = 0x8000,
        Reset = 0x80000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        LargePages = 0x20000000
    }

    public enum InjectionResult
    {
        DllNotFound,
        ProcessNotFound,
        Failed,
        Success
    }

    public sealed class Injector
    {
        static readonly IntPtr INTPTR_ZERO = (IntPtr)0;

        #region Interops
        [Flags]
        internal enum MoveFileFlags
        {
            None = 0,
            ReplaceExisting = 1,
            CopyAllowed = 2,
            DelayUntilReboot = 4,
            WriteThrough = 8,
            CreateHardlink = 16,
            FailIfNotTrackable = 32,
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, MemoryProtectionConsts flNewProtect, int lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, AllocationType dwFreeType);
        #endregion

        #region Constructors
        static Injector _instance;

        public static Injector GetInstance
        {
            get
            {
                if (_instance == null)
                    _instance = new Injector();

                return _instance;
            }
        }

        Injector() { }
        #endregion

        #region Constants
        const UInt32 INFINITE = 0xFFFFFFFF;
        const UInt32 WAIT_ABANDONED = 0x00000080;
        const UInt32 WAIT_OBJECT_0 = 0x00000000;
        const UInt32 WAIT_TIMEOUT = 0x00000102;
        #endregion

        public InjectionResult Inject(string sProcName, string sDllPath)
        {
            if (!File.Exists(sDllPath))
                return InjectionResult.DllNotFound;

            uint processId = 0;
            int index = 0;

            Process[] procs = Process.GetProcesses();
            for (index = 0; index < procs.Length; index++)
            {
                if (procs[index].ProcessName == sProcName)
                {
                    processId = (uint)procs[index].Id;
                    break;
                }
            }

            if (processId == 0)
                return InjectionResult.ProcessNotFound;

            if (!bInject(processId, sDllPath))
                return InjectionResult.Failed;

            return InjectionResult.Success;
        }

        #region Private 
        private int ErasePEHeader(IntPtr hModule, string procName)
        {
            try
            {
                byte[] ImageNTHeaderPointer = new byte[4];
                byte[] Stub = new byte[120];
                byte[] Stub2 = new byte[0x108];
                int Out = 0;
                int Out2 = 0;

                IntPtr process = OpenProcess(0x001F0FFF, false, (uint)Process.GetProcessesByName(procName)[0].Id);
                IntPtr IMAGE_NT_HEADER = new IntPtr((hModule.ToInt32() + 60));
                IntPtr out2 = IntPtr.Zero;

                ReadProcessMemory(process, IMAGE_NT_HEADER, ImageNTHeaderPointer, 4, out out2);
                if ((WriteProcessMemory(process, hModule, Stub, 120, out Out) == 0) && (WriteProcessMemory(process, hModule, Stub2, 0x100, out Out2) == 0))
                    return Out + Out2;
                else
                    return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");

                return 0;
            }
        }

        private bool bInject(uint pToBeInjected, string sDllPath)
        {
            try
            {
                IntPtr hProcess = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), false, pToBeInjected);
                if (hProcess == INTPTR_ZERO)
                    return false;

                ErasePEHeader(hProcess, "GTA5");

                IntPtr pLoadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                if (pLoadLibraryAddress == INTPTR_ZERO)
                    return false;

                IntPtr pAddress = VirtualAllocEx(hProcess, (IntPtr)null, (IntPtr)sDllPath.Length, (0x1000 | 0x2000), 0X40);
                if (pAddress == INTPTR_ZERO)
                    return false;

                byte[] bytes = Encoding.ASCII.GetBytes(sDllPath);

                int count = 0;
                if (WriteProcessMemory(hProcess, pAddress, bytes, (uint)bytes.Length, out count) == 0)
                    return false;

                IntPtr hThread = CreateRemoteThread(hProcess, (IntPtr)null, INTPTR_ZERO, pLoadLibraryAddress, pAddress, 0, (IntPtr)null);
                if (hThread == INTPTR_ZERO)
                    return false;

                bool isRunning = false;
                if (hThread != INTPTR_ZERO)
                    isRunning = WaitForSingleObject(hThread, 10000) != WAIT_TIMEOUT;

                VirtualFreeEx(hProcess, pAddress, sDllPath.Length, AllocationType.Release);

                MoveFileEx(sDllPath, null, MoveFileFlags.DelayUntilReboot);

                CloseHandle(hProcess);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");

                return false;
            }
        }
        #endregion
    }
}