using System;
using dotnet_helper;
using Foundation;
using ObjCRuntime;

public class Inject
{
    delegate void imp_text(IntPtr self, IntPtr sel, IntPtr str);
    static imp_text old_text;
    static void new_text(IntPtr self, IntPtr sel, IntPtr str)
    {
        string s = NSString.FromHandle(str);
        ObjcHelper.NSLog("hook: " + s);
        old_text(self, sel, str);
    }
    public static void Entry()
    {
        old_text = ObjcHook.MethodHook<imp_text>(ObjcHelper.GetClass("test"), new Selector("text:"), new_text);
    }
}