using System;
using DirectN;

namespace WicNet;

public class WicMetadataKeyValue(WicMetadataKey key, object value, PropertyType type)
{
    public WicMetadataKey Key { get; } = key ?? throw new ArgumentNullException(nameof(key));
    public object Value { get; } = value;

    // keeping the exact type is important for example to differentiate between VT_BLOB and VT_UI1 | VT_VECTOR
    public PropertyType Type { get; } = type;

    public override string ToString() => Key + ": " + Value + " (" + Type + ")";
}
