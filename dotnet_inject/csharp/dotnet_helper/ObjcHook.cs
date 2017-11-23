using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ObjCRuntime;

namespace dotnet_helper
{
    public class ObjcHook
    {
        private static IDictionary<string, MethodHookInfo> _mHookList = new Dictionary<string, MethodHookInfo>();

        public static T MethodHook<T>(Class cls, Selector sel, T imp)
            where T : class
        {
            if (imp is Delegate)
            {
                MethodHookInfo info;
                string name = cls.Name + "|" + sel.Name;
                if (_mHookList.ContainsKey(name))
                {
                    info = _mHookList[name];
                    info.hook_imp = imp as Delegate;
                    Objc.class_replaceMethod(cls.Handle, sel.Handle, Marshal.GetFunctionPointerForDelegate(info.hook_imp), "@:");
                }
                else
                {
                    info = new MethodHookInfo();
                    info.imp_type = typeof(T);
                    info.hook_imp = imp as Delegate;
                    info.old_imp = Objc.class_replaceMethod(cls.Handle, sel.Handle, Marshal.GetFunctionPointerForDelegate(info.hook_imp), "@:");
                    _mHookList.Add(name, info);
                }
                if (info.old_imp != IntPtr.Zero)
                    return Marshal.GetDelegateForFunctionPointer(info.old_imp, typeof(T)) as T;
            }
            return null;
        }
        public static void MethodUnhook(Class cls, Selector sel)
        {
            string name = cls.Name + "|" + sel.Name;
            if (_mHookList.ContainsKey(name))
            {
                MethodHookInfo info = _mHookList[name];
                Objc.class_replaceMethod(cls.Handle, sel.Handle, info.old_imp, "@:");
                _mHookList.Remove(name);
            }
        }
        private class MethodHookInfo
        {
            public Type imp_type;
            public Delegate hook_imp;
            public IntPtr old_imp;
        }
    }
}
