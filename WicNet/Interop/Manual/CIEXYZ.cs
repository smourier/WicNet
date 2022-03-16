using System.Runtime.InteropServices;

namespace WicNet.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CIEXYZ
    {
        public int ciexyzX;
        public int ciexyzY;
        public int ciexyzZ;
    }
}
