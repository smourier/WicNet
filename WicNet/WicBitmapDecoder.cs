using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using WicNet.Interop;

namespace WicNet
{
    public sealed class WicBitmapDecoder : IDisposable, IEnumerable<WicBitmapSource>
    {
        private readonly IComObject<IWICBitmapDecoder> _comObject;

        public WicBitmapDecoder(object comObject)
        {
            _comObject = new ComObjectWrapper<IWICBitmapDecoder>(comObject).ComObject;
        }

        public IComObject<IWICBitmapDecoder> ComObject => _comObject;
        public int FrameCount => _comObject.GetFrameCount();
        public Guid ContainerFormat => _comObject.GetContainerFormat();

        public WicBitmapSource GetFrame(int index = 0) => new WicBitmapSource(_comObject.GetFrame(index));

        public WicBitmapSource GetPreview()
        {
            var bmp = _comObject.GetPreview();
            return bmp != null ? new WicBitmapSource(bmp) : null;
        }

        public WicBitmapSource GetThumbnail()
        {
            var bmp = _comObject.GetThumbnail();
            return bmp != null ? new WicBitmapSource(bmp) : null;
        }

        public WicMetadataQueryReader GetMetadataQueryReader()
        {
            var reader = _comObject.GetMetadataQueryReader();
            return reader != null ? new WicMetadataQueryReader(reader) : null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<WicBitmapSource> GetEnumerator() => EnumerateFrames().GetEnumerator();

        public IEnumerable<WicBitmapSource> EnumerateFrames()
        {
            for (var i = 0; i < FrameCount; i++)
            {
                yield return GetFrame(i);
            }
        }

        public static WicBitmapDecoder Load(string filePath, Guid? guidVendor = null, FileAccess access = FileAccess.Read, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException(null, filePath);

            return new WicBitmapDecoder(WICImagingFactory.CreateDecoderFromFilename(filePath, guidVendor, access, metadataOptions: options));
        }

        public static WicBitmapDecoder Load(IntPtr fileHandle, Guid? guidVendor = null, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (fileHandle == null)
                throw new ArgumentNullException(nameof(fileHandle));

            return new WicBitmapDecoder(WICImagingFactory.CreateDecoderFromFileHandle(fileHandle, guidVendor, options));
        }

        public static WicBitmapDecoder Load(Stream stream, Guid? guidVendor = null, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new WicBitmapDecoder(WICImagingFactory.CreateDecoderFromStream(stream, guidVendor, options));
        }

        public static WicBitmapDecoder Load(IStream stream, Guid? guidVendor = null, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new WicBitmapDecoder(WICImagingFactory.CreateDecoderFromStream(stream, guidVendor, options));
        }

        public void Dispose() => _comObject?.Dispose();
    }
}
