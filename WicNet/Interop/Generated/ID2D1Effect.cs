// c:\program files (x86)\windows kits\10\include\10.0.22000.0\um\d2d1_1.h(1299,1)
using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    /// <summary>
    /// The effect interface. Properties control how the effect is rendered. The effect is Drawn with the DrawImage call.
    /// </summary>
    [ComImport, Guid("28211a43-7d89-476f-8181-2d6159b220ad"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface ID2D1Effect : ID2D1Properties
    {
        // ID2D1Properties
        [PreserveSig]
        new int GetPropertyCount();
        
        [PreserveSig]
        new HRESULT GetPropertyName(int index, /* _Out_writes_(nameCount) */ [MarshalAs(UnmanagedType.LPWStr)] string name, int nameCount);
        
        [PreserveSig]
        new int GetPropertyNameLength(int index);
        
        [PreserveSig]
        new D2D1_PROPERTY_TYPE GetType(int index);
        
        [PreserveSig]
        new int GetPropertyIndex(/* _In_ */ [MarshalAs(UnmanagedType.LPWStr)] string name);
        
        [PreserveSig]
        new HRESULT SetValueByName(/* _In_ */ [MarshalAs(UnmanagedType.LPWStr)] string name, D2D1_PROPERTY_TYPE type, /* _In_reads_(dataSize) */ [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] data, int dataSize);
        
        [PreserveSig]
        new HRESULT SetValue(int index, D2D1_PROPERTY_TYPE type, /* _In_reads_(dataSize) */ [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] data, int dataSize);
        
        [PreserveSig]
        new HRESULT GetValueByName(/* _In_ */ [MarshalAs(UnmanagedType.LPWStr)] string name, D2D1_PROPERTY_TYPE type, /* _Out_writes_(dataSize) */ [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] data, int dataSize);
        
        [PreserveSig]
        new HRESULT GetValue(int index, D2D1_PROPERTY_TYPE type, /* _Out_writes_(dataSize) */ [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] data, int dataSize);
        
        [PreserveSig]
        new int GetValueSize(int index);
        
        [PreserveSig]
        new HRESULT GetSubProperties(int index, /* _COM_Outptr_result_maybenull_ */ out ID2D1Properties subProperties);
        
        // ID2D1Effect
        [PreserveSig]
        void SetInput(int index, /* _In_opt_ */ ID2D1Image input, bool invalidate);
        
        [PreserveSig]
        HRESULT SetInputCount(int inputCount);
        
        [PreserveSig]
        void GetInput(int index, /* _Outptr_result_maybenull_ */ out ID2D1Image input);
        
        [PreserveSig]
        int GetInputCount();
        
        [PreserveSig]
        void GetOutput(/* _Outptr_ */ out ID2D1Image outputImage);
    }
}
