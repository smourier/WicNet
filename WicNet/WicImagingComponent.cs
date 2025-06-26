using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DirectN;
using Microsoft.Win32;
using WicNet.Utilities;

namespace WicNet;

public class WicImagingComponent : IEquatable<WicImagingComponent>
{
    private static readonly Lazy<Dictionary<Guid, WicImagingComponent>> _allComponents = new(LoadAllComponents, true);
    private static readonly Lazy<HashSet<string>> _encoderExtensions = new(GetEncoderExtensions, true);
    private static readonly Lazy<HashSet<string>> _decoderExtensions = new(GetDecoderExtensions, true);

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
                        ic = new WicPixelFormat(component);
                        break;

                    case WICComponentType.WICEncoder:
                        ic = new WicEncoder(component);
                        break;

                    case WICComponentType.WICDecoder:
                        ic = new WicDecoder(component);
                        break;

                    case WICComponentType.WICPixelFormatConverter:
                        var converter = new WicPixelFormatConverter(component);
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

                if (!dic.ContainsKey(ic.Clsid))
                {
                    dic.Add(ic.Clsid, ic);
                }
                component.Dispose();
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

    // these should be IReadOnlySet<string> (but it's not in .NET standard...)
    public static ISet<string> DecoderFileExtensions => _decoderExtensions.Value;
    public static ISet<string> EncoderFileExtensions => _encoderExtensions.Value;

    protected WicImagingComponent(object comObject)
    {
        var wrapper = new ComObjectWrapper<IWICComponentInfo>(comObject);
        wrapper.Object.GetCLSID(out Guid guid);
        Clsid = guid;

        wrapper.Object.GetSigningStatus(out var status);
        SigningStatus = (WICComponentSigning)status;

        wrapper.Object.GetComponentType(out var type);
        Type = type;

        FriendlyName = Utilities.Extensions.GetString((s, capacity) =>
        {
            wrapper.Object.GetFriendlyName(capacity, s, out var size);
            return size;
        });

        Author = Utilities.Extensions.GetString((s, capacity) =>
        {
            wrapper.Object.GetAuthor(capacity, s, out var size);
            return size;
        });

        Version = Utilities.Extensions.GetString((s, capacity) =>
        {
            wrapper.Object.GetVersion(capacity, s, out var size);
            return size;
        });

        SpecVersion = Utilities.Extensions.GetString((s, capacity) =>
        {
            wrapper.Object.GetSpecVersion(capacity, s, out var size);
            return size;
        });

        using (var key = Registry.ClassesRoot.OpenSubKey(Path.Combine("CLSID", Clsid.ToString("B"))))
        {
            if (key != null)
            {
                if (key.GetValue(nameof(ArbitrationPriority)) is int priority)
                {
                    ArbitrationPriority = priority;
                }

                if (key.GetValue(nameof(Vendor)) is string vendorText && Guid.TryParse(vendorText, out var vendor))
                {
                    Vendor = vendor;
                }
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
    public int ArbitrationPriority { get; }
    public Guid Vendor { get; }
    public virtual string ClsidName => GetClassName(Clsid);

    public override string ToString() => FriendlyName + " " + ClsidName;
    public override int GetHashCode() => Clsid.GetHashCode();
    public override bool Equals(object obj) => Equals(obj as WicImagingComponent);
    public bool Equals(WicImagingComponent other) => other != null && Clsid == other.Clsid;

    public static bool operator !=(WicImagingComponent left, WicImagingComponent right)
    {
        if (left is null)
            return right is not null;

        return !left.Equals(right);
    }

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

    public static string GetClassName(Guid guid) => typeof(WicImagingComponent).GetGuidName(guid);

    public static readonly Guid CLSID_WICAdngDecoder = new("981d9411-909e-42a7-8f5d-a747ff052edb");
    public static readonly Guid CLSID_WICBmpDecoder = new("6b462062-7cbf-400d-9fdb-813dd10f2778");
    public static readonly Guid CLSID_WICBmpEncoder = new("69be8bb4-d66d-47c8-865a-ed1589433782");
    public static readonly Guid CLSID_WICDdsDecoder = new("9053699f-a341-429d-9e90-ee437cf80c73");
    public static readonly Guid CLSID_WICDdsEncoder = new("a61dde94-66ce-4ac1-881b-71680588895e");
    public static readonly Guid CLSID_WICDefaultFormatConverter = new("1a3f11dc-b514-4b17-8c5f-2154513852f1");
    public static readonly Guid CLSID_WICFormatConverterHighColor = new("ac75d454-9f37-48f8-b972-4e19bc856011");
    public static readonly Guid CLSID_WICFormatConverterNChannel = new("c17cabb2-d4a3-47d7-a557-339b2efbd4f1");
    public static readonly Guid CLSID_WICFormatConverterWMPhoto = new("9cb5172b-d600-46ba-ab77-77bb7e3a00d9");
    public static readonly Guid CLSID_WICGifDecoder = new("381dda3c-9ce9-4834-a23e-1f98f8fc52be");
    public static readonly Guid CLSID_WICGifEncoder = new("114f5598-0b22-40a0-86a1-c83ea495adbd");
    public static readonly Guid CLSID_WICHeifDecoder = new("e9a4a80a-44fe-4de4-8971-7150b10a5199");
    public static readonly Guid CLSID_WICHeifEncoder = new("0dbecec1-9eb3-4860-9c6f-ddbe86634575");
    public static readonly Guid CLSID_WICIcoDecoder = new("c61bfcdf-2e0f-4aad-a8d7-e06bafebcdfe");
    public static readonly Guid CLSID_WICImagingCategories = new("fae3d380-fea4-4623-8c75-c6b61110b681");
    public static readonly Guid CLSID_WICImagingFactory1 = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
    public static readonly Guid CLSID_WICImagingFactory2 = new("317d06e8-5f24-433d-bdf7-79ce68d8abc2");
    public static readonly Guid CLSID_WICJpegDecoder = new("9456a480-e88b-43ea-9e73-0b2d9b71b1ca");
    public static readonly Guid CLSID_WICJpegEncoder = new("1a34f5c1-4a5a-46dc-b644-1f4567e7a676");
    public static readonly Guid CLSID_WICJpegQualcommPhoneEncoder = new("68ed5c62-f534-4979-b2b3-686a12b2b34c");
    public static readonly Guid CLSID_WICPlanarFormatConverter = new("184132b8-32f8-4784-9131-dd7224b23438");
    public static readonly Guid CLSID_WICPngDecoder1 = new("389ea17b-5078-4cde-b6ef-25c15175c751");
    public static readonly Guid CLSID_WICPngDecoder2 = new("e018945b-aa86-4008-9bd4-6777a1e40c11");
    public static readonly Guid CLSID_WICPngEncoder = new("27949969-876a-41d7-9447-568f6a35a4dc");
    public static readonly Guid CLSID_WICRAWDecoder = new("41945702-8302-44a6-9445-ac98e8afa086");
    public static readonly Guid CLSID_WICTiffDecoder = new("b54e85d9-fe23-499f-8b88-6acea713752b");
    public static readonly Guid CLSID_WICTiffEncoder = new("0131be10-2001-4c5f-a9b0-cc88fab64ce8");
    public static readonly Guid CLSID_WICWebpDecoder = new("7693e886-51c9-4070-8419-9f70738ec8fa");
    public static readonly Guid CLSID_WICWmpDecoder = new("a26cec36-234c-4950-ae16-e34aace71d0d");
    public static readonly Guid CLSID_WICWmpEncoder = new("ac4ce3cb-e1c1-44cd-8215-5a1665509ec2");

    public static readonly Guid CLSID_WIC8BIMIPTCDigestMetadataReader = new("02805f1e-d5aa-415b-82c5-61c033a988a6");
    public static readonly Guid CLSID_WIC8BIMIPTCDigestMetadataWriter = new("2db5e62b-0d67-495f-8f9d-c2f0188647ac");
    public static readonly Guid CLSID_WIC8BIMIPTCMetadataReader = new("0010668c-0801-4da6-a4a4-826522b6d28f");
    public static readonly Guid CLSID_WIC8BIMIPTCMetadataWriter = new("00108226-ee41-44a2-9e9c-4be4d5b1d2cd");
    public static readonly Guid CLSID_WIC8BIMResolutionInfoMetadataReader = new("5805137a-e348-4f7c-b3cc-6db9965a0599");
    public static readonly Guid CLSID_WIC8BIMResolutionInfoMetadataWriter = new("4ff2fe0e-e74a-4b71-98c4-ab7dc16707ba");
    public static readonly Guid CLSID_WICAPEMetadataReader = new("1767b93a-b021-44ea-920f-863c11f4f768");
    public static readonly Guid CLSID_WICAPEMetadataWriter = new("bd6edfca-2890-482f-b233-8d7339a1cf8d");
    public static readonly Guid CLSID_WICApp0MetadataReader = new("43324b33-a78f-480f-9111-9638aaccc832");
    public static readonly Guid CLSID_WICApp0MetadataWriter = new("f3c633a2-46c8-498e-8fbb-cc6f721bbcde");
    public static readonly Guid CLSID_WICApp13MetadataReader = new("aa7e3c50-864c-4604-bc04-8b0b76e637f6");
    public static readonly Guid CLSID_WICApp13MetadataWriter = new("7b19a919-a9d6-49e5-bd45-02c34e4e4cd5");
    public static readonly Guid CLSID_WICApp1MetadataReader = new("dde33513-774e-4bcd-ae79-02f4adfe62fc");
    public static readonly Guid CLSID_WICApp1MetadataWriter = new("ee366069-1832-420f-b381-0479ad066f19");
    public static readonly Guid CLSID_WICDdsMetadataReader = new("276c88ca-7533-4a86-b676-66b36080d484");
    public static readonly Guid CLSID_WICDdsMetadataWriter = new("fd688bbd-31ed-4db7-a723-934927d38367");
    public static readonly Guid CLSID_WICExifMetadataReader = new("d9403860-297f-4a49-bf9b-77898150a442");
    public static readonly Guid CLSID_WICExifMetadataWriter = new("c9a14cda-c339-460b-9078-d4debcfabe91");
    public static readonly Guid CLSID_WICGCEMetadataReader = new("b92e345d-f52d-41f3-b562-081bc772e3b9");
    public static readonly Guid CLSID_WICGCEMetadataWriter = new("af95dc76-16b2-47f4-b3ea-3c31796693e7");
    public static readonly Guid CLSID_WICGifCommentMetadataReader = new("32557d3b-69dc-4f95-836e-f5972b2f6159");
    public static readonly Guid CLSID_WICGifCommentMetadataWriter = new("a02797fc-c4ae-418c-af95-e637c7ead2a1");
    public static readonly Guid CLSID_WICGpsMetadataReader = new("3697790b-223b-484e-9925-c4869218f17a");
    public static readonly Guid CLSID_WICGpsMetadataWriter = new("cb8c13e4-62b5-4c96-a48b-6ba6ace39c76");
    public static readonly Guid CLSID_WICHeifHDRMetadataReader = new("2438de3d-94d9-4be8-84a8-4de95a575e75");
    public static readonly Guid CLSID_WICHeifMetadataReader = new("acddfc3f-85ec-41bc-bdef-1bc262e4db05");
    public static readonly Guid CLSID_WICHeifMetadataWriter = new("3ae45e79-40bc-4401-ace5-dd3cb16e6afe");
    public static readonly Guid CLSID_WICIfdMetadataReader = new("8f914656-9d0a-4eb2-9019-0bf96d8a9ee6");
    public static readonly Guid CLSID_WICIfdMetadataWriter = new("b1ebfc28-c9bd-47a2-8d33-b948769777a7");
    public static readonly Guid CLSID_WICIMDMetadataReader = new("7447a267-0015-42c8-a8f1-fb3b94c68361");
    public static readonly Guid CLSID_WICIMDMetadataWriter = new("8c89071f-452e-4e95-9682-9d1024627172");
    public static readonly Guid CLSID_WICInteropMetadataReader = new("b5c8b898-0074-459f-b700-860d4651ea14");
    public static readonly Guid CLSID_WICInteropMetadataWriter = new("122ec645-cd7e-44d8-b186-2c8c20c3b50f");
    public static readonly Guid CLSID_WICIPTCMetadataReader = new("03012959-f4f6-44d7-9d09-daa087a9db57");
    public static readonly Guid CLSID_WICIPTCMetadataWriter = new("1249b20c-5dd0-44fe-b0b3-8f92c8e6d080");
    public static readonly Guid CLSID_WICIRBMetadataReader = new("d4dcd3d7-b4c2-47d9-a6bf-b89ba396a4a3");
    public static readonly Guid CLSID_WICIRBMetadataWriter = new("5c5c1935-0235-4434-80bc-251bc1ec39c6");
    public static readonly Guid CLSID_WICJpegChrominanceMetadataReader = new("50b1904b-f28f-4574-93f4-0bade82c69e9");
    public static readonly Guid CLSID_WICJpegChrominanceMetadataWriter = new("3ff566f0-6e6b-49d4-96e6-b78886692c62");
    public static readonly Guid CLSID_WICJpegCommentMetadataReader = new("9f66347c-60c4-4c4d-ab58-d2358685f607");
    public static readonly Guid CLSID_WICJpegCommentMetadataWriter = new("e573236f-55b1-4eda-81ea-9f65db0290d3");
    public static readonly Guid CLSID_WICJpegLuminanceMetadataReader = new("356f2f88-05a6-4728-b9a4-1bfbce04d838");
    public static readonly Guid CLSID_WICJpegLuminanceMetadataWriter = new("1d583abc-8a0e-4657-9982-a380ca58fb4b");
    public static readonly Guid CLSID_WICLSDMetadataReader = new("41070793-59e4-479a-a1f7-954adc2ef5fc");
    public static readonly Guid CLSID_WICLSDMetadataWriter = new("73c037e7-e5d9-4954-876a-6da81d6e5768");
    public static readonly Guid CLSID_WICPngBkgdMetadataReader = new("0ce7a4a6-03e8-4a60-9d15-282ef32ee7da");
    public static readonly Guid CLSID_WICPngBkgdMetadataWriter = new("68e3f2fd-31ae-4441-bb6a-fd7047525f90");
    public static readonly Guid CLSID_WICPngChrmMetadataReader = new("f90b5f36-367b-402a-9dd1-bc0fd59d8f62");
    public static readonly Guid CLSID_WICPngChrmMetadataWriter = new("e23ce3eb-5608-4e83-bcef-27b1987e51d7");
    public static readonly Guid CLSID_WICPngGamaMetadataReader = new("3692ca39-e082-4350-9e1f-3704cb083cd5");
    public static readonly Guid CLSID_WICPngGamaMetadataWriter = new("ff036d13-5d4b-46dd-b10f-106693d9fe4f");
    public static readonly Guid CLSID_WICPngHistMetadataReader = new("877a0bb7-a313-4491-87b5-2e6d0594f520");
    public static readonly Guid CLSID_WICPngHistMetadataWriter = new("8a03e749-672e-446e-bf1f-2c11d233b6ff");
    public static readonly Guid CLSID_WICPngIccpMetadataReader = new("f5d3e63b-cb0f-4628-a478-6d8244be36b1");
    public static readonly Guid CLSID_WICPngIccpMetadataWriter = new("16671e5f-0ce6-4cc4-9768-e89fe5018ade");
    public static readonly Guid CLSID_WICPngItxtMetadataReader = new("aabfb2fa-3e1e-4a8f-8977-5556fb94ea23");
    public static readonly Guid CLSID_WICPngItxtMetadataWriter = new("31879719-e751-4df8-981d-68dff67704ed");
    public static readonly Guid CLSID_WICPngSrgbMetadataReader = new("fb40360c-547e-4956-a3b9-d4418859ba66");
    public static readonly Guid CLSID_WICPngSrgbMetadataWriter = new("a6ee35c6-87ec-47df-9f22-1d5aad840c82");
    public static readonly Guid CLSID_WICPngTextMetadataReader = new("4b59afcc-b8c3-408a-b670-89e5fab6fda7");
    public static readonly Guid CLSID_WICPngTextMetadataWriter = new("b5ebafb9-253e-4a72-a744-0762d2685683");
    public static readonly Guid CLSID_WICPngTimeMetadataReader = new("d94edf02-efe5-4f0d-85c8-f5a68b3000b1");
    public static readonly Guid CLSID_WICPngTimeMetadataWriter = new("1ab78400-b5a3-4d91-8ace-33fcd1499be6");
    public static readonly Guid CLSID_WICSubIfdMetadataReader = new("50d42f09-ecd1-4b41-b65d-da1fdaa75663");
    public static readonly Guid CLSID_WICSubIfdMetadataWriter = new("8ade5386-8e9b-4f4c-acf2-f0008706b238");
    public static readonly Guid CLSID_WICThumbnailMetadataReader = new("fb012959-f4f6-44d7-9d09-daa087a9db57");
    public static readonly Guid CLSID_WICThumbnailMetadataWriter = new("d049b20c-5dd0-44fe-b0b3-8f92c8e6d080");
    public static readonly Guid CLSID_WICUnknownMetadataReader = new("699745c2-5066-4b82-a8e3-d40478dbec8c");
    public static readonly Guid CLSID_WICUnknownMetadataWriter = new("a09cca86-27ba-4f39-9053-121fa4dc08fc");
    public static readonly Guid CLSID_WICWebpAnimMetadataReader = new("076f9911-a348-465c-a807-a252f3f2d3de");
    public static readonly Guid CLSID_WICWebpAnmfMetadataReader = new("85a10b03-c9f6-439f-be5e-c0fbef67807c");
    public static readonly Guid CLSID_WICXMPAltMetadataReader = new("aa94dcc2-b8b0-4898-b835-000aabd74393");
    public static readonly Guid CLSID_WICXMPAltMetadataWriter = new("076c2a6c-f78f-4c46-a723-3583e70876ea");
    public static readonly Guid CLSID_WICXMPBagMetadataReader = new("e7e79a30-4f2c-4fab-8d00-394f2d6bbebe");
    public static readonly Guid CLSID_WICXMPBagMetadataWriter = new("ed822c8c-d6be-4301-a631-0e1416bad28f");
    public static readonly Guid CLSID_WICXMPMetadataReader = new("72b624df-ae11-4948-a65c-351eb0829419");
    public static readonly Guid CLSID_WICXMPMetadataWriter = new("1765e14e-1bd4-462e-b6b1-590bf1262ac6");
    public static readonly Guid CLSID_WICXMPSeqMetadataReader = new("7f12e753-fc71-43d7-a51d-92f35977abb5");
    public static readonly Guid CLSID_WICXMPSeqMetadataWriter = new("6d68d1de-d432-4b0f-923a-091183a9bda7");
    public static readonly Guid CLSID_WICXMPStructMetadataReader = new("01b90d9a-8209-47f7-9c52-e1244bf50ced");
    public static readonly Guid CLSID_WICXMPStructMetadataWriter = new("22c21f93-7ddb-411c-9b17-c5b7bd064abc");

    // manual
    public static readonly Guid CLSID_WICCurDecoder = new("22696b76-881b-48d7-88f0-dc6111ff9f0b");
}
