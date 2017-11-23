using System;
using System.Runtime.InteropServices;
using ObjCRuntime;

namespace dotnet_helper
{
    class Objc
    {
        [DllImport(Constants.FoundationLibrary)]
        public static extern void NSLog(IntPtr format);
        [DllImport(Constants.ObjectiveCLibrary)]
        public static extern void free(IntPtr p);
        [DllImport(Constants.ObjectiveCLibrary)]
        public static extern IntPtr objc_getClass(string name);
        [DllImport(Constants.ObjectiveCLibrary)]
        public static extern IntPtr class_replaceMethod(IntPtr cls, IntPtr name, IntPtr imp, string types);
        [DllImport(Constants.ObjectiveCLibrary)]
        public static extern IntPtr class_copyMethodList(IntPtr cls, out int outCount);
        [DllImport(Constants.ObjectiveCLibrary)]
        public static extern IntPtr method_getName(IntPtr m);
    }
}
