using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DirectN;

namespace WicNet
{
    public sealed class WicMetadataQueryReader : IDisposable, IEnumerable<WicMetadataKeyValue>
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

        public string ContainerFormatName => GetFormatName(ContainerFormat);

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

        public override string ToString() => ContainerFormatName + " " + Location;

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

            return Conversions.TryChangeType(obj, out value);
        }

        public bool TryGetMetadataByName(string name, out object value, out PropertyType type)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            using (var pv = new PropVariant())
            {
                if (_comObject.Object.GetMetadataByName(name, pv).IsError)
                {
                    value = null;
                    type = PropertyType.VT_EMPTY;
                    return false;
                }

                value = pv.Value;
                type = pv.VarType;
                return true;
            }
        }

        public IEnumerable<WicMetadataKeyValue> Enumerate(bool recursive = false)
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

        public void Visit(Action<WicMetadataQueryReader, WicMetadataKeyValue> action, bool recursive = true)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var kv in this)
            {
                action(this, kv);
                if (kv.Value is WicMetadataQueryReader reader)
                {
                    if (recursive)
                    {
                        reader.Visit(action, recursive);
                    }
                }
            }
        }

        IEnumerator<WicMetadataKeyValue> IEnumerable<WicMetadataKeyValue>.GetEnumerator()
        {
            foreach (var name in Strings)
            {
                if (name == null)
                    continue;

                if (!TryGetMetadataByName(name, out var value, out var type))
                    continue;

                if (value is IWICMetadataQueryReader reader)
                {
                    var childReader = new WicMetadataQueryReader(reader);
                    yield return new WicMetadataKeyValue(new WicMetadataKey(childReader.ContainerFormat, name), childReader, type);
                }
                else
                {
                    yield return new WicMetadataKeyValue(new WicMetadataKey(ContainerFormat, name), value, type);
                }
            }
        }

        public void Dispose() => _comObject?.Dispose();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<WicMetadataKeyValue>)this).GetEnumerator();

        public static string GetFormatName(Guid guid)
        {
            if (Extensions.TryGetGuidName(typeof(WicMetadataQueryReader), guid, out var name))
                return name;

            return Extensions.GetGuidName(typeof(WicCodec), guid);
        }

        public static readonly Guid GUID_MetadataFormat8BIMIPTC = new Guid("0010568c-0852-4e6a-b191-5c33ac5b0430");
        public static readonly Guid GUID_MetadataFormat8BIMIPTCDigest = new Guid("1ca32285-9ccd-4786-8bd8-79539db6a006");
        public static readonly Guid GUID_MetadataFormat8BIMResolutionInfo = new Guid("739f305d-81db-43cb-ac5e-55013ef9f003");
        public static readonly Guid GUID_MetadataFormatAPE = new Guid("2e043dc2-c967-4e05-875e-618bf67e85c3");
        public static readonly Guid GUID_MetadataFormatApp0 = new Guid("79007028-268d-45d6-a3c2-354e6a504bc9");
        public static readonly Guid GUID_MetadataFormatApp1 = new Guid("8fd3dfc3-f951-492b-817f-69c2e6d9a5b0");
        public static readonly Guid GUID_MetadataFormatApp13 = new Guid("326556a2-f502-4354-9cc0-8e3f48eaf6b5");
        public static readonly Guid GUID_MetadataFormatChunkbKGD = new Guid("e14d3571-6b47-4dea-b60a-87ce0a78dfb7");
        public static readonly Guid GUID_MetadataFormatChunkcHRM = new Guid("9db3655b-2842-44b3-8067-12e9b375556a");
        public static readonly Guid GUID_MetadataFormatChunkgAMA = new Guid("f00935a5-1d5d-4cd1-81b2-9324d7eca781");
        public static readonly Guid GUID_MetadataFormatChunkhIST = new Guid("c59a82da-db74-48a4-bd6a-b69c4931ef95");
        public static readonly Guid GUID_MetadataFormatChunkiCCP = new Guid("eb4349ab-b685-450f-91b5-e802e892536c");
        public static readonly Guid GUID_MetadataFormatChunkiTXt = new Guid("c2bec729-0b68-4b77-aa0e-6295a6ac1814");
        public static readonly Guid GUID_MetadataFormatChunksRGB = new Guid("c115fd36-cc6f-4e3f-8363-524b87c6b0d9");
        public static readonly Guid GUID_MetadataFormatChunktEXt = new Guid("568d8936-c0a9-4923-905d-df2b38238fbc");
        public static readonly Guid GUID_MetadataFormatChunktIME = new Guid("6b00ae2d-e24b-460a-98b6-878bd03072fd");
        public static readonly Guid GUID_MetadataFormatDds = new Guid("4a064603-8c33-4e60-9c29-136231702d08");
        public static readonly Guid GUID_MetadataFormatExif = new Guid("1c3c4f9d-b84a-467d-9493-36cfbd59ea57");
        public static readonly Guid GUID_MetadataFormatGCE = new Guid("2a25cad8-deeb-4c69-a788-0ec2266dcafd");
        public static readonly Guid GUID_MetadataFormatGifComment = new Guid("c4b6e0e0-cfb4-4ad3-ab33-9aad2355a34a");
        public static readonly Guid GUID_MetadataFormatGps = new Guid("7134ab8a-9351-44ad-af62-448db6b502ec");
        public static readonly Guid GUID_MetadataFormatHeif = new Guid("817ef3e1-1288-45f4-a852-260d9e7cce83");
        public static readonly Guid GUID_MetadataFormatHeifHDR = new Guid("568b8d8a-1e65-438c-8968-d60e1012beb9");
        public static readonly Guid GUID_MetadataFormatIfd = new Guid("537396c6-2d8a-4bb6-9bf8-2f0a8e2a3adf");
        public static readonly Guid GUID_MetadataFormatIMD = new Guid("bd2bb086-4d52-48dd-9677-db483e85ae8f");
        public static readonly Guid GUID_MetadataFormatInterop = new Guid("ed686f8e-681f-4c8b-bd41-a8addbf6b3fc");
        public static readonly Guid GUID_MetadataFormatIPTC = new Guid("4fab0914-e129-4087-a1d1-bc812d45a7b5");
        public static readonly Guid GUID_MetadataFormatIRB = new Guid("16100d66-8570-4bb9-b92d-fda4b23ece67");
        public static readonly Guid GUID_MetadataFormatJpegChrominance = new Guid("f73d0dcf-cec6-4f85-9b0e-1c3956b1bef7");
        public static readonly Guid GUID_MetadataFormatJpegComment = new Guid("220e5f33-afd3-474e-9d31-7d4fe730f557");
        public static readonly Guid GUID_MetadataFormatJpegLuminance = new Guid("86908007-edfc-4860-8d4b-4ee6e83e6058");
        public static readonly Guid GUID_MetadataFormatLSD = new Guid("e256031e-6299-4929-b98d-5ac884afba92");
        public static readonly Guid GUID_MetadataFormatSubIfd = new Guid("58a2e128-2db9-4e57-bb14-5177891ed331");
        public static readonly Guid GUID_MetadataFormatThumbnail = new Guid("243dcee9-8703-40ee-8ef0-22a600b8058c");
        public static readonly Guid GUID_MetadataFormatUnknown = new Guid("a45e592f-9078-4a7c-adb5-4edc4fd61b1f");
        public static readonly Guid GUID_MetadataFormatWebpANIM = new Guid("6dc4fda6-78e6-4102-ae35-bcfa1edcc78b");
        public static readonly Guid GUID_MetadataFormatWebpANMF = new Guid("43c105ee-b93b-4abb-b003-a08c0d870471");
        public static readonly Guid GUID_MetadataFormatXMP = new Guid("bb5acc38-f216-4cec-a6c5-5f6e739763a9");
        public static readonly Guid GUID_MetadataFormatXMPAlt = new Guid("7b08a675-91aa-481b-a798-4da94908613b");
        public static readonly Guid GUID_MetadataFormatXMPBag = new Guid("833cca5f-dcb7-4516-806f-6596ab26dce4");
        public static readonly Guid GUID_MetadataFormatXMPSeq = new Guid("63e8df02-eb6c-456c-a224-b25e794fd648");
        public static readonly Guid GUID_MetadataFormatXMPStruct = new Guid("22383cf1-ed17-4e2e-af17-d85b8f6b30d0");
    }
}
