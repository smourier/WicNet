// c:\program files (x86)\windows kits\10\include\10.0.19041.0\um\ocidl.h(6017,9)
using System;
using System.Runtime.InteropServices;

namespace WicNet.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct PROPBAG2
    {
        public PROPBAG2_TYPE dwType;
        public ushort vt;
        public ushort cfType;
        public uint dwHint;
        [MarshalAs(UnmanagedType.LPWStr)] 
        public string pstrName;
        public Guid clsid;
    }
}
