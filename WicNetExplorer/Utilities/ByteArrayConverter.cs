using System;
using System.ComponentModel;
using System.Globalization;
using WicNet.Utilities;

namespace WicNetExplorer.Utilities
{
    public class ByteArrayConverter : TypeConverter
    {
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) => false;
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(byte[]);
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is byte[] bytes)
                return Conversions.ToHexa(bytes, 64);

            return string.Empty;
        }
    }
}
