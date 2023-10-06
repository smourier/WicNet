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
        private readonly string _name;

        public ColorContextModel(WicColorContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _name = context.ToString();
            ExifColorSpace = ((ExifColorSpace)context.ExifColorSpace.GetValueOrDefault()).GetEnumName();
            Type = context.Type.GetEnumName("WICColorContext").Decamelize();
            Profile = context.Profile != null ? new ColorProfileModel(context.Profile) : null;
            _profileBytes = context.ProfileBytes;
        }

        [DisplayName("Exif Color Space")]
        public string ExifColorSpace { get; }

        [ToStringVisitor(Ignore = true)]
        public ColorProfileModel? Profile { get; }
        public string? Type { get; }

        [ToStringVisitor(Ignore = true)]
        [DisplayName("Profile Bytes")]
        [Editor(typeof(ByteArrayEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ByteArrayConverter))]
        public byte[] ProfileBytes { get => _profileBytes; set => throw new NotSupportedException(); } // set for property grid support

        public override string ToString() => _name;
    }
}
