using System;
using ObjCRuntime;
namespace dotnet_helper
{
    public class Method
    {
        private IntPtr _handle;
        public Method(IntPtr m)
        {
            this._handle = m;
        }
        public Selector Name
        {
            get
            {
                IntPtr n = Objc.method_getName(this._handle);
                return new Selector(n);
            }
        }
        public IntPtr Handle
        {
            get
            {
                return this._handle;
            }
        }
    }
}
