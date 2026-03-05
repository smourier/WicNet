using System;
using System.ComponentModel;
using System.Drawing.Design;
using DirectN;
using DirectN.Extensions.Com;
using DirectN.Extensions.Utilities;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class BitmapColorContextModel
{
    private readonly byte[]? _profileBytes;

    public BitmapColorContextModel(IComObject<ID2D1ColorContext> colorContext)
    {
        ArgumentNullException.ThrowIfNull(colorContext);
        ColorSpace = colorContext.Object.GetColorSpace();

        var size = colorContext.Object.GetProfileSize();
        if (size > 0)
        {
            _profileBytes = new byte[size];
            unsafe
            {
                fixed (byte* ptr = _profileBytes)
                {
                    colorContext.Object.GetProfile((nint)ptr, _profileBytes.Length()).ThrowOnError();
                }
            }
        }

        var ctx = colorContext.As<ID2D1ColorContext1>();
        if (ctx != null)
        {
            ColorContextType = ctx.Object.GetColorContextType();
            DxgiColorSpace = ctx.Object.GetDXGIColorSpace();
            ctx.Object.GetSimpleColorProfile(out var profile);
            SimpleColorProfile = profile;
        }
    }

    [DisplayName("Color Space")]
    public D2D1_COLOR_SPACE ColorSpace { get; }

    [DisplayName("Type")]
    public D2D1_COLOR_CONTEXT_TYPE ColorContextType { get; }

    [DisplayName("DXGI Color Space")]
    public DXGI_COLOR_SPACE_TYPE DxgiColorSpace { get; }

    [DisplayName("Simple Color Profile")]
    public D2D1_SIMPLE_COLOR_PROFILE SimpleColorProfile { get; }

    [DisplayName("Profile Bytes")]
    [Editor(typeof(ByteArrayEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(ByteArrayConverter))]
    public byte[]? ProfileBytes { get => _profileBytes; set => throw new NotSupportedException(); } // set for property grid support
}
