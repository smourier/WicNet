// c:\program files (x86)\windows kits\10\include\10.0.19041.0\um\wincodec.h(5598,5)
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace DirectN
{
    [ComImport, Guid("d8cd007f-d08f-4191-9bfc-236ea7f0e4b5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IWICBitmapDecoderInfo : IWICBitmapCodecInfo
    {
        // IWICComponentInfo
        [PreserveSig]
        new HRESULT GetComponentType(/* [out] __RPC__out */ out WICComponentType pType);

        [PreserveSig]
        new HRESULT GetCLSID(/* [out] __RPC__out */ out Guid pclsid);

        [PreserveSig]
        new HRESULT GetSigningStatus(/* [out] __RPC__out */ out WICComponentSigning pStatus);

        [PreserveSig]
        new HRESULT GetAuthor(/* [in] */ int cchAuthor, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchAuthor) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzAuthor, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        new HRESULT GetVendorGUID(/* [out] __RPC__out */ out Guid pguidVendor);

        [PreserveSig]
        new HRESULT GetVersion(/* [in] */ int cchVersion, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchVersion) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzVersion, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        new HRESULT GetSpecVersion(/* [in] */ int cchSpecVersion, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchSpecVersion) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzSpecVersion, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        new HRESULT GetFriendlyName(/* [in] */ int cchFriendlyName, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchFriendlyName) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzFriendlyName, /* [out] __RPC__out */ out int pcchActual);

        // IWICBitmapCodecInfo
        [PreserveSig]
        new HRESULT GetContainerFormat(/* [out] __RPC__out */ out Guid pguidContainerFormat);

        [PreserveSig]
        new HRESULT GetPixelFormats(/* [in] */ int cFormats, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cFormats) */ [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Guid[] pguidPixelFormats, /* [out] __RPC__out */ out int pcActual);

        [PreserveSig]
        new HRESULT GetColorManagementVersion(/* [in] */ int cchColorManagementVersion, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchColorManagementVersion) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzColorManagementVersion, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        new HRESULT GetDeviceManufacturer(/* [in] */ int cchDeviceManufacturer, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchDeviceManufacturer) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzDeviceManufacturer, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        new HRESULT GetDeviceModels(/* [in] */ int cchDeviceModels, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchDeviceModels) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzDeviceModels, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        new HRESULT GetMimeTypes(/* [in] */ int cchMimeTypes, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchMimeTypes) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzMimeTypes, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        new HRESULT GetFileExtensions(/* [in] */ int cchFileExtensions, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchFileExtensions) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzFileExtensions, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        new HRESULT DoesSupportAnimation(/* [out] __RPC__out */ out bool pfSupportAnimation);

        [PreserveSig]
        new HRESULT DoesSupportChromakey(/* [out] __RPC__out */ out bool pfSupportChromakey);

        [PreserveSig]
        new HRESULT DoesSupportLossless(/* [out] __RPC__out */ out bool pfSupportLossless);

        [PreserveSig]
        new HRESULT DoesSupportMultiframe(/* [out] __RPC__out */ out bool pfSupportMultiframe);

        [PreserveSig]
        new HRESULT MatchesMimeType(/* [in] __RPC__in */ [MarshalAs(UnmanagedType.LPWStr)] string wzMimeType, /* [out] __RPC__out */ out bool pfMatches);

        // IWICBitmapDecoderInfo
        [PreserveSig]
        HRESULT GetPatterns(/* [in] */ uint cbSizePatterns, /* optional(WICBitmapPattern) */ IntPtr pPatterns, /* optional(UINT) */ IntPtr pcPatterns, /* [annotation][out] _Out_ */ out uint pcbPatternsActual);
        
        [PreserveSig]
        HRESULT MatchesPattern(/* [in] __RPC__in_opt */ IStream pIStream, /* [out] __RPC__out */ out bool pfMatches);
        
        [PreserveSig]
        HRESULT CreateInstance(/* [out] __RPC__deref_out_opt */ out IWICBitmapDecoder ppIBitmapDecoder);
    }
}
