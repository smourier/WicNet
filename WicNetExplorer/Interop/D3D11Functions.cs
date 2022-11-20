using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    public static class D3D11Functions
    {
        public static object D3D11CreateDevice()
        {
            var flags = D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT;
            var hr = D3D11CreateDevice(
                null,
                 D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
                IntPtr.Zero,
                flags,
                null,
                0,
                D3D11_SDK_VERSION,
                out var device,
                out var _,
                IntPtr.Zero);

            if (hr.IsSuccess)
                return device;

            var hr2 = D3D11CreateDevice(
                null,
                 D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_WARP,
                IntPtr.Zero,
                flags,
                null,
                0,
                D3D11_SDK_VERSION,
                out device,
                out var _,
                IntPtr.Zero);
            if (hr2.IsSuccess)
                return device;

            hr.ThrowOnError(true);
            return device;
        }

        private const int D3D11_SDK_VERSION = 7;

        [DllImport("d3d11", ExactSpelling = true)]
        private static extern HRESULT D3D11CreateDevice(/*IDXGIAdapter*/ [MarshalAs(UnmanagedType.IUnknown)] object? pAdapter, D3D_DRIVER_TYPE DriverType, IntPtr Software, D3D11_CREATE_DEVICE_FLAG Flags, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] D3D_FEATURE_LEVEL[]? pFeatureLevels, uint FeatureLevels, uint SDKVersion, /*ID3D11Device*/ [MarshalAs(UnmanagedType.IUnknown)] out object ppDevice, out D3D_FEATURE_LEVEL pFeatureLevel, /*out ID3D11DeviceContext*/ IntPtr ppImmediateContext);
    }
}
