using System;
using System.Collections.Generic;
using System.Linq;
using DirectN;
using DirectNAot.Extensions.Com;
using DirectNAot.Extensions.Utilities;

namespace WicNet;

public sealed class WicPixelFormat : WicImagingComponent, IComparable, IComparable<WicPixelFormat>
{
    private readonly Lazy<IReadOnlyList<WicPixelFormat>> _possibleTargetFormats;

    public WicPixelFormat(object comObject)
        : base(comObject)
    {
        using (var info = new ComObjectWrapper<IWICPixelFormatInfo>(comObject))
        {
            info.Object.GetFormatGUID(out var guid);
            Guid = guid;

            info.Object.GetChannelCount(out var i);
            ChannelCount = i;

            info.Object.GetBitsPerPixel(out i);
            BitsPerPixel = i;

            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                if (info.Object is IWICPixelFormatInfo2 info2)
                {
                    info2.GetNumericRepresentation(out var nr);
                    NumericRepresentation = nr;

                    info2.SupportsTransparency(out var b);
                    SupportsTransparency = b;
                }
            }
        }
        _possibleTargetFormats = new Lazy<IReadOnlyList<WicPixelFormat>>(GetPossibleTargetFormats);
    }

    public Guid Guid { get; }
    public WICPixelFormatNumericRepresentation NumericRepresentation { get; }
    public uint ChannelCount { get; }
    public uint BitsPerPixel { get; }
    public bool SupportsTransparency { get; }
    public override string ClsidName => GetFormatName(Clsid);
    public IReadOnlyList<WicPixelFormat> PossibleTargetFormats => _possibleTargetFormats.Value;

    public WicColorContext? GetColorContext() => WicImagingFactory.WithFactory(factory =>
    {
        factory.CreateComponentInfo(Clsid, out var info);
        if (info is IWICPixelFormatInfo format)
        {
            format.GetColorContext(out var ctx);
            if (ctx != null)
                return new WicColorContext(ctx);
        }
        return null;
    });

    public static WicPixelFormat? FromName(string name) => FromName<WicPixelFormat>(name);
    public static WicPixelFormat? FromClsid(Guid clsid) => FromClsid<WicPixelFormat>(clsid);
    public IEnumerable<WicPixelFormatConverter> GetPixelFormatConvertersTo(Guid targetFormat) => AllComponents.OfType<WicPixelFormatConverter>().Where(pf => pf.CanConvert(Guid, targetFormat));

    int IComparable.CompareTo(object? obj) => CompareTo(obj as WicPixelFormat);
    public int CompareTo(WicPixelFormat? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (BitsPerPixel != other.BitsPerPixel)
            return BitsPerPixel.CompareTo(other.BitsPerPixel);

        return ChannelCount.CompareTo(other.ChannelCount);
    }

    private IReadOnlyList<WicPixelFormat> GetPossibleTargetFormats()
    {
        var list = new List<WicPixelFormat>();
        var formats = AllComponents.OfType<WicPixelFormat>().ToArray();
        foreach (var converter in AllComponents.OfType<WicPixelFormatConverter>())
        {
            foreach (var to in formats)
            {
                if (to.Guid == Guid)
                    continue;

                if (converter.CanConvert(Guid, to.Guid))
                {
                    list.Add(to);
                }
            }
        }
        return list.AsReadOnly();
    }

    public static bool operator <(WicPixelFormat left, WicPixelFormat right) => left is null ? right is not null : left.CompareTo(right) < 0;
    public static bool operator <=(WicPixelFormat left, WicPixelFormat right) => left is null || left.CompareTo(right) <= 0;
    public static bool operator >(WicPixelFormat left, WicPixelFormat right) => left is not null && left.CompareTo(right) > 0;
    public static bool operator >=(WicPixelFormat left, WicPixelFormat right) => left is null ? right is null : left.CompareTo(right) >= 0;

    public static string GetFormatName(Guid guid) => typeof(WicPixelFormat).GetGuidName(guid);

    public static readonly Guid GUID_WICPixelFormat112bpp6ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc937");
    public static readonly Guid GUID_WICPixelFormat112bpp7Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc92a");
    public static readonly Guid GUID_WICPixelFormat128bpp7ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc938");
    public static readonly Guid GUID_WICPixelFormat128bpp8Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc92b");
    public static readonly Guid GUID_WICPixelFormat128bppPRGBAFloat = new("6fddc324-4e03-4bfe-b185-3d77768dc91a");
    public static readonly Guid GUID_WICPixelFormat128bppRGBAFixedPoint = new("6fddc324-4e03-4bfe-b185-3d77768dc91e");
    public static readonly Guid GUID_WICPixelFormat128bppRGBAFloat = new("6fddc324-4e03-4bfe-b185-3d77768dc919");
    public static readonly Guid GUID_WICPixelFormat128bppRGBFixedPoint = new("6fddc324-4e03-4bfe-b185-3d77768dc941");
    public static readonly Guid GUID_WICPixelFormat128bppRGBFloat = new("6fddc324-4e03-4bfe-b185-3d77768dc91b");
    public static readonly Guid GUID_WICPixelFormat144bpp8ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc939");
    public static readonly Guid GUID_WICPixelFormat16bppBGR555 = new("6fddc324-4e03-4bfe-b185-3d77768dc909");
    public static readonly Guid GUID_WICPixelFormat16bppBGR565 = new("6fddc324-4e03-4bfe-b185-3d77768dc90a");
    public static readonly Guid GUID_WICPixelFormat16bppBGRA5551 = new("05ec7c2b-f1e6-4961-ad46-e1cc810a87d2");
    public static readonly Guid GUID_WICPixelFormat16bppCbCr = new("ff95ba6e-11e0-4263-bb45-01721f3460a4");
    public static readonly Guid GUID_WICPixelFormat16bppCbQuantizedDctCoefficients = new("d2c4ff61-56a5-49c2-8b5c-4c1925964837");
    public static readonly Guid GUID_WICPixelFormat16bppCrQuantizedDctCoefficients = new("2fe354f0-1680-42d8-9231-e73c0565bfc1");
    public static readonly Guid GUID_WICPixelFormat16bppGray = new("6fddc324-4e03-4bfe-b185-3d77768dc90b");
    public static readonly Guid GUID_WICPixelFormat16bppGrayFixedPoint = new("6fddc324-4e03-4bfe-b185-3d77768dc913");
    public static readonly Guid GUID_WICPixelFormat16bppGrayHalf = new("6fddc324-4e03-4bfe-b185-3d77768dc93e");
    public static readonly Guid GUID_WICPixelFormat16bppYQuantizedDctCoefficients = new("a355f433-48e8-4a42-84d8-e2aa26ca80a4");
    public static readonly Guid GUID_WICPixelFormat1bppIndexed = new("6fddc324-4e03-4bfe-b185-3d77768dc901");
    public static readonly Guid GUID_WICPixelFormat24bpp3Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc920");
    public static readonly Guid GUID_WICPixelFormat24bppBGR = new("6fddc324-4e03-4bfe-b185-3d77768dc90c");
    public static readonly Guid GUID_WICPixelFormat24bppRGB = new("6fddc324-4e03-4bfe-b185-3d77768dc90d");
    public static readonly Guid GUID_WICPixelFormat2bppGray = new("6fddc324-4e03-4bfe-b185-3d77768dc906");
    public static readonly Guid GUID_WICPixelFormat2bppIndexed = new("6fddc324-4e03-4bfe-b185-3d77768dc902");
    public static readonly Guid GUID_WICPixelFormat32bpp3ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc92e");
    public static readonly Guid GUID_WICPixelFormat32bpp4Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc921");
    public static readonly Guid GUID_WICPixelFormat32bppBGR = new("6fddc324-4e03-4bfe-b185-3d77768dc90e");
    public static readonly Guid GUID_WICPixelFormat32bppBGR101010 = new("6fddc324-4e03-4bfe-b185-3d77768dc914");
    public static readonly Guid GUID_WICPixelFormat32bppBGRA = new("6fddc324-4e03-4bfe-b185-3d77768dc90f");
    public static readonly Guid GUID_WICPixelFormat32bppCMYK = new("6fddc324-4e03-4bfe-b185-3d77768dc91c");
    public static readonly Guid GUID_WICPixelFormat32bppGrayFixedPoint = new("6fddc324-4e03-4bfe-b185-3d77768dc93f");
    public static readonly Guid GUID_WICPixelFormat32bppGrayFloat = new("6fddc324-4e03-4bfe-b185-3d77768dc911");
    public static readonly Guid GUID_WICPixelFormat32bppPBGRA = new("6fddc324-4e03-4bfe-b185-3d77768dc910");
    public static readonly Guid GUID_WICPixelFormat32bppPRGBA = new("3cc4a650-a527-4d37-a916-3142c7ebedba");
    public static readonly Guid GUID_WICPixelFormat32bppR10G10B10A2 = new("604e1bb5-8a3c-4b65-b11c-bc0b8dd75b7f");
    public static readonly Guid GUID_WICPixelFormat32bppR10G10B10A2HDR10 = new("9c215c5d-1acc-4f0e-a4bc-70fb3ae8fd28");
    public static readonly Guid GUID_WICPixelFormat32bppRGB = new("d98c6b95-3efe-47d6-bb25-eb1748ab0cf1");
    public static readonly Guid GUID_WICPixelFormat32bppRGBA = new("f5c7ad2d-6a8d-43dd-a7a8-a29935261ae9");
    public static readonly Guid GUID_WICPixelFormat32bppRGBA1010102 = new("25238d72-fcf9-4522-b514-5578e5ad55e0");
    public static readonly Guid GUID_WICPixelFormat32bppRGBA1010102XR = new("00de6b9a-c101-434b-b502-d0165ee1122c");
    public static readonly Guid GUID_WICPixelFormat32bppRGBE = new("6fddc324-4e03-4bfe-b185-3d77768dc93d");
    public static readonly Guid GUID_WICPixelFormat40bpp4ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc92f");
    public static readonly Guid GUID_WICPixelFormat40bpp5Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc922");
    public static readonly Guid GUID_WICPixelFormat40bppCMYKAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc92c");
    public static readonly Guid GUID_WICPixelFormat48bpp3Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc926");
    public static readonly Guid GUID_WICPixelFormat48bpp5ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc930");
    public static readonly Guid GUID_WICPixelFormat48bpp6Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc923");
    public static readonly Guid GUID_WICPixelFormat48bppBGR = new("e605a384-b468-46ce-bb2e-36f180e64313");
    public static readonly Guid GUID_WICPixelFormat48bppBGRFixedPoint = new("49ca140e-cab6-493b-9ddf-60187c37532a");
    public static readonly Guid GUID_WICPixelFormat48bppRGB = new("6fddc324-4e03-4bfe-b185-3d77768dc915");
    public static readonly Guid GUID_WICPixelFormat48bppRGBFixedPoint = new("6fddc324-4e03-4bfe-b185-3d77768dc912");
    public static readonly Guid GUID_WICPixelFormat48bppRGBHalf = new("6fddc324-4e03-4bfe-b185-3d77768dc93b");
    public static readonly Guid GUID_WICPixelFormat4bppGray = new("6fddc324-4e03-4bfe-b185-3d77768dc907");
    public static readonly Guid GUID_WICPixelFormat4bppIndexed = new("6fddc324-4e03-4bfe-b185-3d77768dc903");
    public static readonly Guid GUID_WICPixelFormat56bpp6ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc931");
    public static readonly Guid GUID_WICPixelFormat56bpp7Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc924");
    public static readonly Guid GUID_WICPixelFormat64bpp3ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc934");
    public static readonly Guid GUID_WICPixelFormat64bpp4Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc927");
    public static readonly Guid GUID_WICPixelFormat64bpp7ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc932");
    public static readonly Guid GUID_WICPixelFormat64bpp8Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc925");
    public static readonly Guid GUID_WICPixelFormat64bppBGRA = new("1562ff7c-d352-46f9-979e-42976b792246");
    public static readonly Guid GUID_WICPixelFormat64bppBGRAFixedPoint = new("356de33c-54d2-4a23-bb04-9b7bf9b1d42d");
    public static readonly Guid GUID_WICPixelFormat64bppCMYK = new("6fddc324-4e03-4bfe-b185-3d77768dc91f");
    public static readonly Guid GUID_WICPixelFormat64bppPBGRA = new("8c518e8e-a4ec-468b-ae70-c9a35a9c5530");
    public static readonly Guid GUID_WICPixelFormat64bppPRGBA = new("6fddc324-4e03-4bfe-b185-3d77768dc917");
    public static readonly Guid GUID_WICPixelFormat64bppPRGBAHalf = new("58ad26c2-c623-4d9d-b320-387e49f8c442");
    public static readonly Guid GUID_WICPixelFormat64bppRGB = new("a1182111-186d-4d42-bc6a-9c8303a8dff9");
    public static readonly Guid GUID_WICPixelFormat64bppRGBA = new("6fddc324-4e03-4bfe-b185-3d77768dc916");
    public static readonly Guid GUID_WICPixelFormat64bppRGBAFixedPoint = new("6fddc324-4e03-4bfe-b185-3d77768dc91d");
    public static readonly Guid GUID_WICPixelFormat64bppRGBAHalf = new("6fddc324-4e03-4bfe-b185-3d77768dc93a");
    public static readonly Guid GUID_WICPixelFormat64bppRGBFixedPoint = new("6fddc324-4e03-4bfe-b185-3d77768dc940");
    public static readonly Guid GUID_WICPixelFormat64bppRGBHalf = new("6fddc324-4e03-4bfe-b185-3d77768dc942");
    public static readonly Guid GUID_WICPixelFormat72bpp8ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc933");
    public static readonly Guid GUID_WICPixelFormat80bpp4ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc935");
    public static readonly Guid GUID_WICPixelFormat80bpp5Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc928");
    public static readonly Guid GUID_WICPixelFormat80bppCMYKAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc92d");
    public static readonly Guid GUID_WICPixelFormat8bppAlpha = new("e6cd0116-eeba-4161-aa85-27dd9fb3a895");
    public static readonly Guid GUID_WICPixelFormat8bppCb = new("1339f224-6bfe-4c3e-9302-e4f3a6d0ca2a");
    public static readonly Guid GUID_WICPixelFormat8bppCr = new("b8145053-2116-49f0-8835-ed844b205c51");
    public static readonly Guid GUID_WICPixelFormat8bppGray = new("6fddc324-4e03-4bfe-b185-3d77768dc908");
    public static readonly Guid GUID_WICPixelFormat8bppIndexed = new("6fddc324-4e03-4bfe-b185-3d77768dc904");
    public static readonly Guid GUID_WICPixelFormat8bppY = new("91b4db54-2df9-42f0-b449-2909bb3df88e");
    public static readonly Guid GUID_WICPixelFormat96bpp5ChannelsAlpha = new("6fddc324-4e03-4bfe-b185-3d77768dc936");
    public static readonly Guid GUID_WICPixelFormat96bpp6Channels = new("6fddc324-4e03-4bfe-b185-3d77768dc929");
    public static readonly Guid GUID_WICPixelFormat96bppRGBFixedPoint = new("6fddc324-4e03-4bfe-b185-3d77768dc918");
    public static readonly Guid GUID_WICPixelFormat96bppRGBFloat = new("e3fed78f-e8db-4acf-84c1-e97f6136b327");
    public static readonly Guid GUID_WICPixelFormatBlackWhite = new("6fddc324-4e03-4bfe-b185-3d77768dc905");
    public static readonly Guid GUID_WICPixelFormatDontCare = new("6fddc324-4e03-4bfe-b185-3d77768dc900");
    public static readonly Guid GUID_WICPixelFormatUndefined = new("6fddc324-4e03-4bfe-b185-3d77768dc900");
}
