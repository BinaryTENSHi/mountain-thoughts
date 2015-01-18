using System;
using System.Diagnostics;

namespace mountain_thoughts
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Process mountainProcess = NativeMethods.GetMountainProcess();
            if (mountainProcess == null)
            {
                Console.WriteLine("Could not get process. Is Mountain running?");
                Console.ReadLine();
                Environment.Exit(1);
            }

            IntPtr handle = mountainProcess.Handle;
            IntPtr address = NativeMethods.GetStringAddress(mountainProcess);

            NativeMethods.StartReadingString(handle, address, Callback);
            Console.ReadLine();
            NativeMethods.StopReadingString();
        }

        private static void Callback(string thought)
        {
            Console.WriteLine(thought);
        }
    }
}