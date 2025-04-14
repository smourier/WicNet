using System;
using System.ComponentModel;
using System.Globalization;

namespace WicNetExplorer.Utilities;

public class ArrayDisplayExpandableConverter : ExpandableObjectConverter
{
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) => ArrayDisplayConverter.ConvertTo(value);
}
