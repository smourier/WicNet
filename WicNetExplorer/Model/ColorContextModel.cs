using System;
using System.ComponentModel;
using System.Drawing.Design;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ColorContextModel
    {
        private readonly byte[] _profileBytes;

        public ColorContextModel(WicColorContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            ExifColorSpace = ((ExifColorSpace)context.ExifColorSpace.GetValueOrDefault()).GetEnumName();
            Type = context.Type.GetEnumName("WICColorContext").Decamelize();
            Profile = context.Profile != null ? new ColorProfileModel(context.Profile) : null;
            _profileBytes = context.ProfileBytes;
        }

        [DisplayName("Exif Color Space")]
        public string ExifColorSpace { get; }

        public ColorProfileModel? Profile { get; }
        public string? Type { get; }

        [DisplayName("Profile Bytes")]
        [Editor(typeof(ByteArrayEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ByteArrayConverter))]
        public byte[] ProfileBytes { get => _profileBytes; set => throw new NotSupportedException(); } // set for property grid support

        public override string ToString() => Profile?.ToString() ?? string.Empty;
    }
}
