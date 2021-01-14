using System;
using System.Collections.Generic;
using System.Linq;
using WicNet.Interop;

namespace WicNet
{
#pragma warning disable CA1036 // Override methods on comparable types
    public sealed class WicPixelFormat : WicImagingComponent, IComparable, IComparable<WicPixelFormat>
#pragma warning restore CA1036 // Override methods on comparable types
    {
        internal static readonly Guid Format32bppPBGRA = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910");
        internal static readonly Guid Format32bppBGRA = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90f");
        internal static readonly Guid Format24bppBGR = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90c");
        internal static readonly Guid Format16bppBGR555 = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc909");
        internal static readonly Guid Format8bppIndexed = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc904");

        public WicPixelFormat(object comObject)
            : base(comObject)
        {
            using (var info = new ComObjectWrapper<IWICPixelFormatInfo2>(comObject))
            {
                info.Object.GetFormatGUID(out var guid);
                Guid = guid;

                info.Object.GetChannelCount(out var i);
                ChannelCount = (int)i;

                info.Object.GetBitsPerPixel(out i);
                BitsPerPixel = (int)i;

                info.Object.GetNumericRepresentation(out var nr);
                NumericRepresentation = nr;

                info.Object.SupportsTransparency(out bool b);
                SupportsTransparency = b;
            }
        }

        public Guid Guid { get; }
        public WICPixelFormatNumericRepresentation NumericRepresentation { get; }
        public int ChannelCount { get; }
        public int BitsPerPixel { get; }
        public bool SupportsTransparency { get; }

        public static WicPixelFormat FromName(string name) => FromName<WicPixelFormat>(name);
        public static WicPixelFormat FromClsid(Guid clsid) => FromClsid<WicPixelFormat>(clsid);
        public IEnumerable<WicPixelFormatConverter> GetPixelFormatConvertersTo(Guid targetFormat) => AllComponents.OfType<WicPixelFormatConverter>().Where(pf => pf.CanConvert(Guid, targetFormat));

        int IComparable.CompareTo(object obj) => CompareTo(obj as WicPixelFormat);
        public int CompareTo(WicPixelFormat other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (BitsPerPixel != other.BitsPerPixel)
                return BitsPerPixel.CompareTo(other.BitsPerPixel);

            return ChannelCount.CompareTo(other.ChannelCount);
        }

        public static bool operator <(WicPixelFormat left, WicPixelFormat right) => left is null ? right is object : left.CompareTo(right) < 0;
        public static bool operator <=(WicPixelFormat left, WicPixelFormat right) => left is null || left.CompareTo(right) <= 0;
        public static bool operator >(WicPixelFormat left, WicPixelFormat right) => left is object && left.CompareTo(right) > 0;
        public static bool operator >=(WicPixelFormat left, WicPixelFormat right) => left is null ? right is null : left.CompareTo(right) >= 0;
    }
}
