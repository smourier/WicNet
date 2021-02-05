using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace WicNet.Interop
{
    public static class IWICImagingFactoryExtensions
    {
        public static IComObject<IWICPalette> CreatePalette(this IComObject<IWICImagingFactory> factory) => CreatePalette(factory?.Object);
        public static IComObject<IWICPalette> CreatePalette(this IWICImagingFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreatePalette(out var value).ThrowOnError();
            return new ComObject<IWICPalette>(value);
        }

        public static IComObject<IWICBitmap> CreateBitmap(this IComObject<IWICImagingFactory> factory, int width, int height, Guid pixelFormat, WICBitmapCreateCacheOption option = WICBitmapCreateCacheOption.WICBitmapNoCache) => CreateBitmap(factory?.Object, width, height, pixelFormat, option);
        public static IComObject<IWICBitmap> CreateBitmap(this IWICImagingFactory factory, int width, int height, Guid pixelFormat, WICBitmapCreateCacheOption option = WICBitmapCreateCacheOption.WICBitmapNoCache)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreateBitmap(width, height, pixelFormat, option, out var value).ThrowOnError();
            return new ComObject<IWICBitmap>(value);
        }

        public static IComObject<IWICBitmapClipper> CreateBitmapClipper(this IComObject<IWICImagingFactory> factory) => CreateBitmapClipper(factory?.Object);
        public static IComObject<IWICBitmapClipper> CreateBitmapClipper(this IWICImagingFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreateBitmapClipper(out var value).ThrowOnError();
            return new ComObject<IWICBitmapClipper>(value);
        }

        public static IComObject<IWICBitmapFlipRotator> CreateBitmapFlipRotator(this IComObject<IWICImagingFactory> factory) => CreateBitmapFlipRotator(factory?.Object);
        public static IComObject<IWICBitmapFlipRotator> CreateBitmapFlipRotator(this IWICImagingFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreateBitmapFlipRotator(out var value).ThrowOnError();
            return new ComObject<IWICBitmapFlipRotator>(value);
        }

        public static IComObject<IWICBitmapScaler> CreateBitmapScaler(this IComObject<IWICImagingFactory> factory) => CreateBitmapScaler(factory?.Object);
        public static IComObject<IWICBitmapScaler> CreateBitmapScaler(this IWICImagingFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreateBitmapScaler(out var value).ThrowOnError();
            return new ComObject<IWICBitmapScaler>(value);
        }

        public static IComObject<IWICBitmapDecoder> CreateDecoderFromFilename(this IComObject<IWICImagingFactory> factory, string fileName, Guid? guidVendor = null, FileAccess desiredAccess = FileAccess.Read, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand) => CreateDecoderFromFilename(factory?.Object, fileName, guidVendor, desiredAccess, metadataOptions);
        public static IComObject<IWICBitmapDecoder> CreateDecoderFromFilename(this IWICImagingFactory factory, string fileName, Guid? guidVendor = null, FileAccess desiredAccess = FileAccess.Read, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (fileName == null)
                throw new ArgumentNullException(nameof(factory));

            const uint GENERIC_READ = 0x80000000;
            const uint GENERIC_WRITE = 0x40000000;
            using (var guid = new ComMemory(guidVendor))
            {
                uint acc = 0;
                if (desiredAccess.HasFlag(FileAccess.Read))
                {
                    acc |= GENERIC_READ;
                }

                if (desiredAccess.HasFlag(FileAccess.Read))
                {
                    acc |= GENERIC_WRITE;
                }

                factory.CreateDecoderFromFilename(fileName, guid.Pointer, acc, metadataOptions, out var value).ThrowOnError();
                return new ComObject<IWICBitmapDecoder>(value);
            }
        }

        public static IComObject<IWICBitmapDecoder> CreateDecoderFromStream(this IComObject<IWICImagingFactory> factory, IStream stream, Guid? guidVendor = null, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand) => CreateDecoderFromStream(factory?.Object, stream, guidVendor, metadataOptions);
        public static IComObject<IWICBitmapDecoder> CreateDecoderFromStream(this IWICImagingFactory factory, IStream stream, Guid? guidVendor = null, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (stream == null)
                throw new ArgumentNullException(nameof(factory));

            using (var guid = new ComMemory(guidVendor))
            {
                factory.CreateDecoderFromStream(stream, guid.Pointer, metadataOptions, out var value).ThrowOnError();
                return new ComObject<IWICBitmapDecoder>(value);
            }
        }

        public static IComObject<IWICBitmapDecoder> CreateDecoderFromFileHandle(this IComObject<IWICImagingFactory> factory, IntPtr handle, Guid? guidVendor = null, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand) => CreateDecoderFromFileHandle(factory?.Object, handle, guidVendor, metadataOptions);
        public static IComObject<IWICBitmapDecoder> CreateDecoderFromFileHandle(this IWICImagingFactory factory, IntPtr handle, Guid? guidVendor = null, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            using (var guid = new ComMemory(guidVendor))
            {
                factory.CreateDecoderFromFileHandle(handle, guid.Pointer, metadataOptions, out var value).ThrowOnError();
                return new ComObject<IWICBitmapDecoder>(value);
            }
        }

        public static IComObject<IEnumUnknown> CreateComponentEnumerator(this IComObject<IWICImagingFactory> factory, WICComponentType type = WICComponentType.WICAllComponents, WICComponentEnumerateOptions options = WICComponentEnumerateOptions.WICComponentEnumerateDefault) => CreateComponentEnumerator(factory?.Object, type, options);
        public static IComObject<IEnumUnknown> CreateComponentEnumerator(this IWICImagingFactory factory, WICComponentType type = WICComponentType.WICAllComponents, WICComponentEnumerateOptions options = WICComponentEnumerateOptions.WICComponentEnumerateDefault)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreateComponentEnumerator(type, options, out var value).ThrowOnError();
            return new ComObject<IEnumUnknown>(value);
        }

        public static IEnumerable<IComObject<IWICComponentInfo>> EnumerateComponents(this IWICImagingFactory factory, WICComponentType type, WICComponentEnumerateOptions options)
        {
            using (var enumerator = factory.CreateComponentEnumerator(type, options))
            {
                do
                {
                    var o = new object[1];
                    var fetched = 0;
                    enumerator.Object.Next(1, o, ref fetched);
                    if (fetched != 1)
                        break;

                    if (o[0] is IWICComponentInfo info)
                        yield return new ComObject<IWICComponentInfo>(info);
                }
                while (true);
            }
        }
    }
}
