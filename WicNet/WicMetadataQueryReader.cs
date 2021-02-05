using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WicNet.Interop;

namespace WicNet
{
    public sealed class WicMetadataQueryReader : IDisposable, IEnumerable<KeyValuePair<string, object>>
    {
        private readonly IComObject<IWICMetadataQueryReader> _comObject;

        public WicMetadataQueryReader(object comObject)
        {
            _comObject = new ComObjectWrapper<IWICMetadataQueryReader>(comObject).ComObject;
        }

        public IComObject<IWICMetadataQueryReader> ComObject => _comObject;

        public string Location
        {
            get
            {
                _comObject.Object.GetLocation(0, null, out var size);
                if (size <= 0)
                    return null;

                var sb = new StringBuilder(size);
                _comObject.Object.GetLocation(sb.Capacity, sb, out _);
                return sb.ToString();
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
                        list.Add(strings[0]);
                    }
                }
                return list.AsReadOnly();
            }
        }

        public T GetMetadataByName<T>(string name, T defaultValue = default)
        {
            if (TryGetMetadataByName<T>(name, out var value))
                return value;

            return defaultValue;
        }

        public object GetMetadataByName(string name, object defaultValue = null)
        {
            if (TryGetMetadataByName(name, out var value))
                return value;

            return defaultValue;
        }

        public bool TryGetMetadataByName<T>(string name, out T value)
        {
            if (!TryGetMetadataByName(name, out var obj))
            {
                value = default;
                return false;
            }

            return Conversions.TryChangeType(obj, out value);
        }

        public bool TryGetMetadataByName(string name, out object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            using (var pv = new PropVariant())
            {
                if (_comObject.Object.GetMetadataByName(name, pv).IsError)
                {
                    value = null;
                    return false;
                }

                value = pv.Value;
                return true;
            }
        }

        public IEnumerable<KeyValuePair<string, object>> Enumerate(bool recursive = true)
        {
            foreach (var kv in this)
            {
                yield return kv;
                if (kv.Value is WicMetadataQueryReader reader)
                {
                    if (recursive)
                    {
                        foreach (var childKv in reader.Enumerate(true))
                        {
                            yield return childKv;
                        }
                    }
                }
            }
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            foreach (var name in Strings)
            {
                if (!TryGetMetadataByName(name, out var value))
                    continue;

                if (value is IWICMetadataQueryReader reader)
                    yield return new KeyValuePair<string, object>(name, new WicMetadataQueryReader(reader));
                else
                    yield return new KeyValuePair<string, object>(name, value);
            }
        }

        public void Dispose() => _comObject?.Dispose();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<string, object>>)this).GetEnumerator();
    }
}
