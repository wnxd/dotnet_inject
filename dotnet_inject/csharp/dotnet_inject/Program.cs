using System;
using System.Runtime.InteropServices;

namespace dotnet_inject
{
    class MainClass
    {
        [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "dlopen")]
        internal static extern IntPtr dlopen(string path, int mode);
        [DllImport("/usr/lib/libSystem.dylib")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);
        [DllImport("/usr/lib/libSystem.dylib")]
        public static extern int dlclose(IntPtr handle);
        //[DllImport("dotnet_inject.dylib")]
        //static extern bool dotnet_inject(int pid, string lib);
        delegate bool dotnet_inject(int pid, string lib);
        public static void Main(string[] args)
        {
            Console.Write("enter pid: ");
            int pid;
            int.TryParse(Console.ReadLine(), out pid);
            Console.Write("enter lib: ");
            string lib = Console.ReadLine().Trim();
            IntPtr handle = dlopen("dotnet_inject.dylib", 2);
            IntPtr func = dlsym(handle, "dotnet_inject");
            dotnet_inject dotnet_inject = (dotnet_inject)Marshal.GetDelegateForFunctionPointer(func, typeof(dotnet_inject));
            Console.WriteLine("inject: " + dotnet_inject(pid, lib));
            dlclose(handle);
            Console.ReadLine();
        }
    }
}
