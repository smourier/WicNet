using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace WicNetExplorer.Utilities
{
    public class ArrayDisplayConverter : ArrayConverter
    {
        public static object? ConvertTo(TypeConverter? converter, ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is IValueProvider valueProvider)
            {
                value = valueProvider.Value;
            }

            if (value is IEnumerable enumerable)
            {
                var s = string.Join(", ", enumerable.OfType<object>().Take(32).Select(o => o?.ToString()));
                if (value is Array array && array.Rank == 1)
                {
                    var max = Settings.Current.MaxArrayElementDisplayed;
                    if (array.Length > max)
                    {
                        s += "... (size: " + array.Length + ")";
                    }
                }
                return s;
            }

            return string.Empty;
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) => ConvertTo(this, context, culture, value, destinationType);
    }
}
