// c:\program files (x86)\windows kits\10\include\10.0.19041.0\um\wincodec.h(5888,5)
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WicNet.Interop
{
    [ComImport, Guid("e8eda601-3d48-431a-ab44-69059be88bbe"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IWICPixelFormatInfo : IWICComponentInfo
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

        // IWICPixelFormatInfo
        [PreserveSig]
        HRESULT GetFormatGUID(/* [out] __RPC__out */ out Guid pFormat);
        
        [PreserveSig]
        HRESULT GetColorContext(/* [out] __RPC__deref_out_opt */ out IWICColorContext ppIColorContext);
        
        [PreserveSig]
        HRESULT GetBitsPerPixel(/* [out] __RPC__out */ out uint puiBitsPerPixel);
        
        [PreserveSig]
        HRESULT GetChannelCount(/* [out] __RPC__out */ out uint puiChannelCount);
        
        [PreserveSig]
        HRESULT GetChannelMask(/* [in] */ uint uiChannelIndex, /* [in] */ int cbMaskBuffer, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cbMaskBuffer) */ [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pbMaskBuffer, /* [out] __RPC__out */ out uint pcbActual);
    }
}
