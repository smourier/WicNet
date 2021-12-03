// c:\program files (x86)\windows kits\10\include\10.0.19041.0\um\wincodec.h(5074,5)
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WicNet.Interop
{
    [ComImport, Guid("e87a44c4-b76e-4c47-8b09-298eb12a2714"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IWICBitmapCodecInfo : IWICComponentInfo
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
        HRESULT GetContainerFormat(/* [out] __RPC__out */ out Guid pguidContainerFormat);

        [PreserveSig]
        HRESULT GetPixelFormats(/* [in] */ int cFormats, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cFormats) */ [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Guid[] pguidPixelFormats, /* [out] __RPC__out */ out int pcActual);

        [PreserveSig]
        HRESULT GetColorManagementVersion(/* [in] */ int cchColorManagementVersion, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchColorManagementVersion) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzColorManagementVersion, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        HRESULT GetDeviceManufacturer(/* [in] */ int cchDeviceManufacturer, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchDeviceManufacturer) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzDeviceManufacturer, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        HRESULT GetDeviceModels(/* [in] */ int cchDeviceModels, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchDeviceModels) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzDeviceModels, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        HRESULT GetMimeTypes(/* [in] */ int cchMimeTypes, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchMimeTypes) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzMimeTypes, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        HRESULT GetFileExtensions(/* [in] */ int cchFileExtensions, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchFileExtensions) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzFileExtensions, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        HRESULT DoesSupportAnimation(/* [out] __RPC__out */ out bool pfSupportAnimation);

        [PreserveSig]
        HRESULT DoesSupportChromakey(/* [out] __RPC__out */ out bool pfSupportChromakey);

        [PreserveSig]
        HRESULT DoesSupportLossless(/* [out] __RPC__out */ out bool pfSupportLossless);

        [PreserveSig]
        HRESULT DoesSupportMultiframe(/* [out] __RPC__out */ out bool pfSupportMultiframe);

        [PreserveSig]
        HRESULT MatchesMimeType(/* [in] __RPC__in */ [MarshalAs(UnmanagedType.LPWStr)] string wzMimeType, /* [out] __RPC__out */ out bool pfMatches);
    }
}
