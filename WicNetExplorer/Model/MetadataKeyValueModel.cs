using System;
using System.ComponentModel;
using DirectN;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
[ToStringVisitor(ForceIsValue = true)]
public class MetadataKeyValueModel : IValueProvider
{
    private readonly WicMetadataKeyValue _kv;

    public MetadataKeyValueModel(WicMetadataKeyValue kv)
    {
        ArgumentNullException.ThrowIfNull(kv);
        _kv = kv;
    }

    public string Name => _kv.Key.Key;

    [DisplayName("Property Type")]
    public PropertyType Type => _kv.Type;

    [DisplayName("Clr Type")]
    public string? ClrType => _kv.Value?.GetType().Name;

    object? IValueProvider.Value => _kv.Value;

    public override string ToString() => _kv.Value?.ToString() ?? string.Empty;
}
