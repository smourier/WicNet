﻿// c:\program files (x86)\windows kits\10\include\10.0.19041.0\um\wincodec.h(1430,5)
using System;
using System.Runtime.InteropServices;
using REFWICPixelFormatGUID = System.Guid;
using WICPixelFormatGUID = System.Guid;

namespace WicNet.Interop
{
    [Guid("00000301-a8f2-4877-ba0a-fd2b6645fb94"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IWICFormatConverter : IWICBitmapSource
    {
        // IWICBitmapSource
        [PreserveSig]
        new HRESULT GetSize(/* [out] __RPC__out */ out int puiWidth, /* [out] __RPC__out */ out int puiHeight);

        [PreserveSig]
        new HRESULT GetPixelFormat(/* [out] __RPC__out */ out WICPixelFormatGUID pPixelFormat);

        [PreserveSig]
        new HRESULT GetResolution(/* [out] __RPC__out */ out double pDpiX, /* [out] __RPC__out */ out double pDpiY);

        [PreserveSig]
        new HRESULT CopyPalette(/* [in] __RPC__in_opt */ IWICPalette pIPalette);

        [PreserveSig]
        new HRESULT CopyPixels(/* optional(WICRect) */ IntPtr prc, /* [in] */ int cbStride, /* [in] */ int cbBufferSize, /* [size_is][out] __RPC__out_ecount_full(cbBufferSize) */ IntPtr pbBuffer);

        // IWICFormatConverter
        [PreserveSig]
        HRESULT Initialize(/* [in] __RPC__in_opt */ IWICBitmapSource pISource, /* [in] __RPC__in */ [MarshalAs(UnmanagedType.LPStruct)] REFWICPixelFormatGUID dstFormat, /* [in] */ WICBitmapDitherType dither, /* [unique][in] __RPC__in_opt */ IWICPalette pIPalette, /* [in] */ double alphaThresholdPercent, /* [in] */ WICBitmapPaletteType paletteTranslate);

        [PreserveSig]
        HRESULT CanConvert(/* [in] __RPC__in */ [MarshalAs(UnmanagedType.LPStruct)] REFWICPixelFormatGUID srcPixelFormat, /* [in] __RPC__in */ [MarshalAs(UnmanagedType.LPStruct)] REFWICPixelFormatGUID dstPixelFormat, /* [out] __RPC__out */ out bool pfCanConvert);
    }
}
