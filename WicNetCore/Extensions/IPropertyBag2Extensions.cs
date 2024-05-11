using System;
using System.Collections.Generic;
using DirectN;
using DirectNAot.Extensions.Com;
using DirectNAot.Extensions.Utilities;

namespace WicNet.Extensions;

public static class IPropertyBag2Extensions
{
    public static void Write(this IComObject<IPropertyBag2> bag, IEnumerable<KeyValuePair<string, object>> properties) => Write(bag?.Object, properties);
    public static void Write(this IPropertyBag2 bag, IEnumerable<KeyValuePair<string, object>> properties)
    {
        ArgumentNullException.ThrowIfNull(bag);

        if (properties == null)
            return;

        foreach (var kv in properties)
        {
            var i = GetIndex(bag, kv.Key);
            if (i < 0) // ?
                continue;

            // read info
            var values = new VARIANT[1];
            using var name = new Pwstr(kv.Key);
            props[0].pstrName = name;
            bag.GetPropertyInfo((uint)i, 1, out var props, out _).ThrowOnError();

            var value = props[0].ChangeType(kv.Value);
            values[0] = value;
            bag.Write(1, props, values).ThrowOnError();
        }
    }

    private static int GetIndex(IPropertyBag2 bag, string name)
    {
        if (bag == null || name == null)
            return -1;

        bag.CountProperties(out var count);
        for (uint i = 0; i < count; i++)
        {
            bag.GetPropertyInfo(i, 1, out var props, out _).ThrowOnError();
            if (props[0].pstrName.EqualsIgnoreCase(name))
                return i;
        }
        return -1;
    }
}
