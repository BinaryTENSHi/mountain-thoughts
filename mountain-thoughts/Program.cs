using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace mountain_thoughts
{
    public class Program
    {
        private static bool _printed;
        private static string _prevString;

        public static int Main(string[] args)
        {
            Process process = Process.GetProcessesByName("Mountain").FirstOrDefault();
            ;
            if (process == null)
            {
                Console.WriteLine("Could not find Mountain process.");
                return 1;
            }

            IntPtr handle = process.Handle;
            IntPtr baseAddress = process.MainModule.BaseAddress;

            Console.WriteLine("Handle: 0x" + handle.ToString("X"));
            Console.WriteLine("Base: 0x" + baseAddress.ToString("X"));

            byte[] ptrBuffer = new byte[4];
            IntPtr byteread = new IntPtr(-1);
            IntPtr address = IntPtr.Add(baseAddress, MemoryValues.Base);

            List<int> offsets = new List<int> {MemoryValues.Base};
            offsets.AddRange(MemoryValues.Offsets);

            foreach (int offset in MemoryValues.Offsets)
            {
                Console.WriteLine("Reading Address 0x" + address.ToString("X"));
                NativeMethods.ReadProcessMemory(handle, address, ptrBuffer, ptrBuffer.Length, out byteread);
                Debug.Assert(byteread.ToInt32() == 4);
                address = (IntPtr) (BitConverter.ToInt32(ptrBuffer, 0) + offset);
            }

            Console.WriteLine("Final Address 0x" + address.ToString("X"));

            byte[] stringBuffer = new byte[64];

            do
            {
                NativeMethods.ReadProcessMemory(handle, address, stringBuffer, stringBuffer.Length, out byteread);
                if (stringBuffer[0] != 0x00 && ParseString(stringBuffer) && !_printed)
                {
                    Console.WriteLine(_prevString);
                    _printed = true;
                }
                Thread.Sleep(100);
            } while (true);
        }

        private static bool ParseString(byte[] stringBuffer)
        {
            string currString = Encoding.ASCII.GetString(stringBuffer).Split('\0')[0];
            if (_prevString == currString)
                return true;
            _printed = false;
            _prevString = currString;
            return false;
        }
    }
}