using System;
using System.ComponentModel;
using System.Globalization;

namespace WicNetExplorer.Utilities
{
    public class ArrayCountConverter : ArrayConverter
    {
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Array array)
                return string.Format(Resources.ArrayLength, array.Length);

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
