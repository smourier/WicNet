using System;
using System.Collections.Generic;
using DirectN;

namespace WicNet;

public sealed class WicMetadataQueryWriter(object comObject) : IDisposable
{
    private readonly IComObject<IWICMetadataQueryWriter> _comObject = new ComObjectWrapper<IWICMetadataQueryWriter>(comObject).ComObject;

    public IComObject<IWICMetadataQueryWriter> ComObject => _comObject;

    public string HandlerFriendlyName => WicMetadataHandler.FriendlyNameFromGuid(ContainerFormat);
    public string ContainerFormatName => GetFormatName(ContainerFormat);

    public string Location
    {
        get
        {
            return Utilities.Extensions.GetString((s, capacity) =>
            {
                _comObject.Object.GetLocation(capacity, s, out var size);
                return size;
            });
        }
    }

    public Guid ContainerFormat
    {
        get
        {
            _comObject.Object.GetContainerFormat(out var format);
            return format;
        }
    }

    public IReadOnlyList<string> Strings
    {
        get
        {
            var list = new List<string>();
            _comObject.Object.GetEnumerator(out var enumString);
            if (enumString != null)
            {
                var strings = new string[1];
                while (enumString.Next(1, strings, IntPtr.Zero) == 0)
                {
                    if (strings[0] != null)
                    {
                        list.Add(strings[0]);
                    }
                }
            }
            return list.AsReadOnly();
        }
    }

    public override string ToString() => ContainerFormatName + Location;

    public void SetMetadataByName(string name, object value, PropertyType? type = null) => _comObject.SetMetadataByName(name, value, type);
    public void RemoveMetadataByName(string name) => _comObject.RemoveMetadataByName(name);

    public T GetMetadataByName<T>(string name, T defaultValue = default) => GetMetadataByName<T>(name, out _, defaultValue);
    public T GetMetadataByName<T>(string name, out PropertyType type, T defaultValue = default)
    {
        if (TryGetMetadataByName<T>(name, out var value, out type))
            return value;

        return defaultValue;
    }

    public object GetMetadataByName(string name, out PropertyType type, object defaultValue = null)
    {
        if (TryGetMetadataByName(name, out var value, out type))
            return value;

        return defaultValue;
    }

    public bool TryGetMetadataByName<T>(string name, out T value, out PropertyType type)
    {
        if (!TryGetMetadataByName(name, out var obj, out type))
        {
            value = default;
            return false;
        }

        return WicMetadataQueryReader.TryChangeType(obj, out value);
    }

    public bool TryGetMetadataByName(string name, out object value, out PropertyType type)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        var detached = new PROPVARIANT();
        if (_comObject.Object.GetMetadataByName(name, detached).IsError)
        {
            value = null;
            type = PropertyType.VT_EMPTY;
            return false;
        }

        using (var pv = detached.Attach())
        {
            value = pv.Value;
            type = pv.VarType;
            return true;
        }
    }

    public void Dispose() => _comObject.SafeDispose();

    public static string GetFormatName(Guid guid)
    {
        if (typeof(WicMetadataQueryReader).TryGetGuidName(guid, out var name))
            return name;

        return typeof(WicCodec).GetGuidName(guid);
    }
}
