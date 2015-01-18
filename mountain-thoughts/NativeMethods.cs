using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace mountain_thoughts
{
    public static class NativeMethods
    {
        private static bool _called;
        private static string _lastString;

        private static readonly string[] Exclusions =
        {
            string.Empty,
            "SAVING... "
        };

        private static bool _reading;
        private static Thread _readingThread;

        public static Process GetMountainProcess()
        {
            return Process.GetProcessesByName("Mountain").FirstOrDefault();
        }

        public static IntPtr GetStringAddress(Process process)
        {
            IntPtr handle = process.Handle;
            IntPtr baseAddress = process.MainModule.BaseAddress;

            byte[] ptrBuffer = new byte[4];
            IntPtr byteread = new IntPtr(-1);
            IntPtr address = IntPtr.Add(baseAddress, MemoryValues.Base);

            foreach (int offset in MemoryValues.Offsets)
            {
                Console.WriteLine("Reading Address 0x" + address.ToString("X"));
                ReadProcessMemory(handle, address, ptrBuffer, ptrBuffer.Length, out byteread);
                Debug.Assert(byteread.ToInt32() == 4);
                address = (IntPtr) (BitConverter.ToInt32(ptrBuffer, 0) + offset);
            }

            return address;
        }

        public static void StartReadingString(IntPtr handle, IntPtr stringAddress, Action<string> act)
        {
            _reading = true;
            _readingThread = new Thread(() => ReadingLoop(handle, stringAddress, act));
            _readingThread.Start();
        }

        public static void StopReadingString()
        {
            _reading = false;
            _readingThread.Join();
        }

        public static void ReadingLoop(IntPtr handle, IntPtr stringAddress, Action<string> act)
        {
            IntPtr byteread = new IntPtr(-1);
            byte[] stringBuffer = new byte[128];

            do
            {
                ReadProcessMemory(handle, stringAddress, stringBuffer, stringBuffer.Length, out byteread);
                if (stringBuffer[0] != 0x00 && ParseString(stringBuffer) && !_called)
                {
                    act(_lastString);
                    _called = true;
                }
                Thread.Sleep(500);
            } while (_reading);
        }

        private static bool ParseString(byte[] stringBuffer)
        {
            string currString = Encoding.ASCII.GetString(stringBuffer).Split('\0')[0];
            currString = currString.Replace("\n", string.Empty).Trim();
            if (_lastString == currString && !Exclusions.Contains(currString))
                return true;
            _called = false;
            _lastString = currString;
            return false;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);
    }
}