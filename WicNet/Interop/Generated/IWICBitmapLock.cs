// c:\program files (x86)\windows kits\10\include\10.0.19041.0\um\wincodec.h(2097,5)
using System;
using System.Runtime.InteropServices;
using WICPixelFormatGUID = System.Guid;

namespace WicNet.Interop
{
    [Guid("00000123-a8f2-4877-ba0a-fd2b6645fb94"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IWICBitmapLock
    {
        [PreserveSig]
        HRESULT GetSize(/* [out] __RPC__out */ out int puiWidth, /* [out] __RPC__out */ out int puiHeight);
        
        [PreserveSig]
        HRESULT GetStride(/* [out] __RPC__out */ out int pcbStride);
        
        [PreserveSig]
        HRESULT GetDataPointer(/* [out] __RPC__out */ out uint pcbBufferSize, /* optional(WICInProcPointer) */ out IntPtr ppbData);
        
        [PreserveSig]
        HRESULT GetPixelFormat(/* [out] __RPC__out */ out WICPixelFormatGUID pPixelFormat);
    }
}
