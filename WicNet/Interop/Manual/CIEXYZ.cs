using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DirectN
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public struct CIEXYZ
    {
        public int ciexyzX;
        public int ciexyzY;
        public int ciexyzZ;

        // for property grid support
        public int X => ciexyzX;
        public int Y => ciexyzY;
        public int Z => ciexyzZ;

        public override string ToString() => "X: " + X + " Y:" + Y + " Z:" + Z;
    }
}
