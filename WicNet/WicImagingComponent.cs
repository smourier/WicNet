using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WicNet.Interop;

namespace WicNet
{
    public class WicImagingComponent : IEquatable<WicImagingComponent>
    {
        private static readonly Lazy<Dictionary<Guid, WicImagingComponent>> _allComponents = new Lazy<Dictionary<Guid, WicImagingComponent>>(LoadAllComponents, true);
        private static readonly Lazy<HashSet<string>> _encoderExtensions = new Lazy<HashSet<string>>(GetEncoderExtensions, true);
        private static readonly Lazy<HashSet<string>> _decoderExtensions = new Lazy<HashSet<string>>(GetDecoderExtensions, true);

        private static Dictionary<Guid, WicImagingComponent> LoadAllComponents()
        {
            var dic = new Dictionary<Guid, WicImagingComponent>();
            WICImagingFactory.WithFactory(f =>
            {
                foreach (var component in f.EnumerateComponents(WICComponentType.WICAllComponents, WICComponentEnumerateOptions.WICComponentEnumerateDefault))
                {
                    component.Object.GetComponentType(out var type);
                    WicImagingComponent ic;
                    switch (type)
                    {
                        case WICComponentType.WICPixelFormat:
                            ic = new WicPixelFormat((IWICPixelFormatInfo2)component);
                            break;

                        case WICComponentType.WICEncoder:
                            ic = new WicEncoder((IWICBitmapCodecInfo)component);
                            break;

                        case WICComponentType.WICDecoder:
                            ic = new WicDecoder((IWICBitmapCodecInfo)component);
                            break;

                        case WICComponentType.WICPixelFormatConverter:
                            var converter = new WicPixelFormatConverter((IWICFormatConverterInfo)component);
                            ic = converter;
                            break;

                        case WICComponentType.WICMetadataReader:
                            var reader = new WicMetadataReader(component);
                            ic = reader;
                            break;

                        case WICComponentType.WICMetadataWriter:
                            var writer = new WicMetadataWriter(component);
                            ic = writer;
                            break;

                        default:
                            ic = new WicImagingComponent(component);
                            break;
                    }

                    dic.Add(ic.Clsid, ic);

                    WicEncoder.CreateBuiltIn();
                    WicDecoder.CreateBuiltIn();
                }
            });
            return dic;
        }

        private static HashSet<string> GetEncoderExtensions()
        {
            var dic = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var enc in AllComponents.OfType<WicEncoder>())
            {
                foreach (var ext in enc.FileExtensionsList)
                {
                    dic.Add(ext);
                }
            }
            return dic;
        }

        private static HashSet<string> GetDecoderExtensions()
        {
            var dic = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dec in AllComponents.OfType<WicDecoder>())
            {
                foreach (var ext in dec.FileExtensionsList)
                {
                    dic.Add(ext);
                }
            }
            return dic;
        }

        public static IEnumerable<WicImagingComponent> AllComponents => _allComponents.Value.Values;
        public static IEnumerable<string> DecoderFileExtensions => _decoderExtensions.Value;
        public static IEnumerable<string> EncoderFileExtensions => _encoderExtensions.Value;

        protected WicImagingComponent(object comObject)
        {
            using (var wrapper = new ComObjectWrapper<IWICComponentInfo>(comObject))
            {
                wrapper.Object.GetCLSID(out Guid guid);
                Clsid = guid;

                wrapper.Object.GetSigningStatus(out var status);
                SigningStatus = status;

                wrapper.Object.GetComponentType(out var type);
                Type = type;

                wrapper.Object.GetFriendlyName(0, null, out var len);
                if (len >= 0)
                {
                    var sb = new StringBuilder(len);
                    wrapper.Object.GetFriendlyName(len + 1, sb, out _);
                    FriendlyName = sb.ToString();
                }

                wrapper.Object.GetAuthor(0, null, out len);
                if (len >= 0)
                {
                    var sb = new StringBuilder(len);
                    wrapper.Object.GetAuthor(len + 1, sb, out _);
                    Author = sb.ToString();
                }

                wrapper.Object.GetVersion(0, null, out len);
                if (len >= 0)
                {
                    var sb = new StringBuilder(len);
                    wrapper.Object.GetVersion(len + 1, sb, out _);
                    Version = sb.ToString();
                }

                wrapper.Object.GetSpecVersion(0, null, out len);
                if (len >= 0)
                {
                    var sb = new StringBuilder(len);
                    wrapper.Object.GetSpecVersion(len + 1, sb, out _);
                    SpecVersion = sb.ToString();
                }
            }
        }

        public Guid Clsid { get; }
        public WICComponentSigning SigningStatus { get; }
        public WICComponentType Type { get; }
        public string FriendlyName { get; internal set; }
        public string Author { get; }
        public string Version { get; }
        public string SpecVersion { get; }

        public override string ToString() => FriendlyName;
        public override int GetHashCode() => Clsid.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as WicImagingComponent);
        public bool Equals(WicImagingComponent other) => other != null && Clsid == other.Clsid;

        public static bool operator !=(WicImagingComponent left, WicImagingComponent right) => left != right;
        public static bool operator ==(WicImagingComponent left, WicImagingComponent right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static T FromName<T>(string name) where T : WicImagingComponent
        {
            if (name == null)
                return null;

            var item = AllComponents.OfType<T>().FirstOrDefault(f => f.FriendlyName.EqualsIgnoreCase(name));
            if (item != null)
                return item;

            return AllComponents.OfType<T>().FirstOrDefault(f => f.FriendlyName.Replace(" ", "").EqualsIgnoreCase(name));
        }

        public static T FromClsid<T>(Guid clsid) where T : WicImagingComponent
        {
            if (!_allComponents.Value.TryGetValue(clsid, out WicImagingComponent ic))
                return null;

            return ic as T;
        }

        public static bool SupportsFileExtensionForEncoding(string ext)
        {
            if (ext == null)
                throw new ArgumentNullException(nameof(ext));

            if (!ext.StartsWith(".", StringComparison.Ordinal))
            {
                ext = Path.GetExtension(ext);
            }
            return EncoderFileExtensions.Contains(ext);
        }

        public static bool SupportsFileExtensionForDecoding(string ext)
        {
            if (ext == null)
                throw new ArgumentNullException(nameof(ext));

            if (!ext.StartsWith(".", StringComparison.Ordinal))
            {
                ext = Path.GetExtension(ext);
            }
            return DecoderFileExtensions.Contains(ext);
        }
    }
}
