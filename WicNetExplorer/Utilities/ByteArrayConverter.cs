﻿using System;
using System.ComponentModel;
using System.Globalization;
using DirectN;

namespace WicNetExplorer.Utilities;

public class ByteArrayConverter : ArrayConverter
{
    public static object? ConvertTo(object? value)
    {
        if (value is IValueProvider valueProvider)
        {
            value = valueProvider.Value;
        }

        if (value is byte[] bytes)
        {
            var max = Settings.Current.MaxArrayElementDisplayed;
            if (bytes.Length > max)
                return bytes.ToHexa(max) + "... (size: " + bytes.Length + ")";

            return bytes.ToHexa();
        }

        return string.Empty;
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) => ConvertTo(value);
}
