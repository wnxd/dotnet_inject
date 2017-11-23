using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace dotnet_helper
{
    public class ObjcHelper
    {
        public static void NSLog(string format, params object[] args)
        {
            NSString s = new NSString(string.Format(format, args));
            Objc.NSLog(s.Handle);
        }
        public static Class GetClass(string name)
        {
            IntPtr cls = Objc.objc_getClass(name);
            if (cls != IntPtr.Zero)
                return new Class(cls);
            return null;
        }
        public static Method[] GetMethods(Class cls)
        {
            List<Method> list = new List<Method>();
            int count;
            IntPtr pList = Objc.class_copyMethodList(cls.Handle, out count);
            if (pList != IntPtr.Zero)
            {
                for (int i = 0; i < count; i++)
                {
                    IntPtr m = Marshal.ReadIntPtr(pList, i * IntPtr.Size);
                    list.Add(new Method(m));
                }
                Objc.free(pList);
            }
            return list.ToArray();
        }
    }
}
