namespace WicNet;

public sealed class WicMetadataQueryWriter(IComObject<IWICMetadataQueryWriter> comObject) : InterlockedComObject<IWICMetadataQueryWriter>(comObject)
{
    public string? HandlerFriendlyName => WicMetadataHandler.FriendlyNameFromGuid(ContainerFormat);
    public string ContainerFormatName => GetFormatName(ContainerFormat);

    public string? Location
    {
        get
        {
            return Utilities.Extensions.GetString((s, capacity) =>
            {
                NativeObject.GetLocation(capacity, s, out var size);
                return size;
            });
        }
    }

    public Guid ContainerFormat
    {
        get
        {
            NativeObject.GetContainerFormat(out var format);
            return format;
        }
    }

    public IReadOnlyList<string> Strings
    {
        get
        {
            var list = new List<string>();
            NativeObject.GetEnumerator(out var enumString);
            if (enumString != null)
            {
                var strings = new PWSTR[1];
                while (enumString.Next(1, strings, 0) == 0)
                {
                    var str = strings[0].ToString();
                    if (str != null)
                    {
                        list.Add(str);
                    }

                    if (strings[0].Value != 0)
                    {
                        Marshal.FreeCoTaskMem(strings[0].Value);
                        strings[0].Value = 0;
                    }
                }
            }
            return list.AsReadOnly();
        }
    }

    public override string ToString() => ContainerFormatName + Location;

    public void SetMetadataByName(string name, object? value, VARENUM? type = null) => NativeObject.SetMetadataByName(name, value, type);
    public void RemoveMetadataByName(string name) => NativeObject.RemoveMetadataByName(name);

    public T? GetMetadataByName<T>(string name, T? defaultValue = default) => GetMetadataByName<T>(name, out _, defaultValue);
    public T? GetMetadataByName<T>(string name, out VARENUM type, T? defaultValue = default)
    {
        if (TryGetMetadataByName<T>(name, out var value, out type))
            return value;

        return defaultValue;
    }

    public object? GetMetadataByName(string name, out VARENUM type, object? defaultValue = null)
    {
        if (TryGetMetadataByName(name, out var value, out type))
            return value;

        return defaultValue;
    }

    public bool TryGetMetadataByName<T>(string name, out T? value, out VARENUM type)
    {
        if (!TryGetMetadataByName(name, out var obj, out type))
        {
            value = default;
            return false;
        }

        return WicMetadataQueryReader.TryChangeType(obj, out value);
    }

    public bool TryGetMetadataByName(string name, out object? value, out VARENUM type)
    {
        ArgumentNullException.ThrowIfNull(name);
        var detached = new PROPVARIANT();
        using var p = new Pwstr(name);
        if (NativeObject.GetMetadataByName(p, ref detached).IsError)
        {
            value = null;
            type = VARENUM.VT_EMPTY;
            return false;
        }

        using var pv = PropVariant.Attach(ref detached);
        value = pv.Value;
        type = pv.VarType;
        return true;
    }

    public static string GetFormatName(Guid guid)
    {
        if (typeof(WicMetadataQueryReader).TryGetGuidName(guid, out var name))
            return name;

        return typeof(WicCodec).GetGuidName(guid);
    }
}
