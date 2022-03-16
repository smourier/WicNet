// c:\program files (x86)\windows kits\10\include\10.0.19041.0\um\wincodec.h(4749,5)
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DirectN
{
    [ComImport, Guid("23bc3f0a-698b-4357-886b-f24d50671334"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IWICComponentInfo
    {
        [PreserveSig]
        HRESULT GetComponentType(/* [out] __RPC__out */ out WICComponentType pType);

        [PreserveSig]
        HRESULT GetCLSID(/* [out] __RPC__out */ out Guid pclsid);

        [PreserveSig]
        HRESULT GetSigningStatus(/* [out] __RPC__out */ out WICComponentSigning pStatus);

        [PreserveSig]
        HRESULT GetAuthor(/* [in] */ int cchAuthor, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchAuthor) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzAuthor, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        HRESULT GetVendorGUID(/* [out] __RPC__out */ out Guid pguidVendor);

        [PreserveSig]
        HRESULT GetVersion(/* [in] */ int cchVersion, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchVersion) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzVersion, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        HRESULT GetSpecVersion(/* [in] */ int cchSpecVersion, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchSpecVersion) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzSpecVersion, /* [out] __RPC__out */ out int pcchActual);

        [PreserveSig]
        HRESULT GetFriendlyName(/* [in] */ int cchFriendlyName, /* [size_is][unique][out][in] __RPC__inout_ecount_full_opt(cchFriendlyName) */ [MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzFriendlyName, /* [out] __RPC__out */ out int pcchActual);
    }
}
