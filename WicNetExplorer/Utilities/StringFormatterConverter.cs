using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace WicNetExplorer.Utilities;

public class StringFormatterConverter : TypeConverter
{
#pragma warning disable IDE0060 // Remove unused parameter
    public static object? ConvertTo(TypeConverter? typeConverter, ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        if (destinationType == typeof(string) && value != null && context != null && context.PropertyDescriptor != null)
        {
            var sf = context.PropertyDescriptor.Attributes.OfType<StringFormatterAttribute>().FirstOrDefault();
            if (sf != null && sf.Format != null)
            {
                var format = sf.Format;
                if (sf.ResourcesType != null)
                {
                    // make sure generated resource type is public
                    var prop = sf.ResourcesType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop != null)
                    {
                        var rm = (ResourceManager)prop.GetValue(null)!;
                        format = rm.GetString(format, culture) ?? sf.Format;
                    }
                }
                return StringFormatter.FormatWith(format, value, sf.ThrowOnError, culture);
            }
        }

        return string.Empty;
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) => ConvertTo(this, context, culture, value, destinationType);
}
