using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DirectN;
using DirectNAot.Extensions;
using DirectNAot.Extensions.Com;

namespace WicNet
{
    public sealed class WicBitmapDecoder(object comObject) : IDisposable, IEnumerable<WicBitmapSource>
    {
        private readonly IComObject<IWICBitmapDecoder> _comObject = new ComObjectWrapper<IWICBitmapDecoder>(comObject).ComObject;

        public IComObject<IWICBitmapDecoder> ComObject => _comObject;
        public uint FrameCount => _comObject.GetFrameCount();
        public Guid ContainerFormat => _comObject.GetContainerFormat();

        public WicBitmapSource GetFrame(int index = 0) => new(_comObject.GetFrame(index));

        public WicBitmapSource? GetPreview()
        {
            var bmp = _comObject.GetPreview();
            return bmp != null ? new WicBitmapSource(bmp) : null;
        }

        public WicBitmapSource? GetThumbnail()
        {
            var bmp = _comObject.GetThumbnail();
            return bmp != null ? new WicBitmapSource(bmp) : null;
        }

        public WicMetadataQueryReader? GetMetadataQueryReader()
        {
            var reader = _comObject.GetMetadataQueryReader();
            return reader != null ? new WicMetadataQueryReader(reader) : null;
        }

        public IReadOnlyList<WicColorContext> GetColorContexts()
        {
            var list = new List<WicColorContext>();
            var contexts = _comObject.GetColorContexts();
            list.AddRange(contexts.Select(cc => new WicColorContext(cc)));
            return list;
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
            ArgumentNullException.ThrowIfNull(filePath);

            if (!File.Exists(filePath))
                throw new FileNotFoundException(null, filePath);

            return new WicBitmapDecoder(WicImagingFactory.CreateDecoderFromFilename(filePath, guidVendor, access, metadataOptions: options));
        }

        public static WicBitmapDecoder Load(nuint fileHandle, Guid? guidVendor = null, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (fileHandle == 0)
                throw new ArgumentNullException(nameof(fileHandle));

            return new WicBitmapDecoder(WicImagingFactory.CreateDecoderFromFileHandle(fileHandle, guidVendor, options));
        }

        public static WicBitmapDecoder Load(Stream stream, Guid? guidVendor = null, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            ArgumentNullException.ThrowIfNull(stream);

            return new WicBitmapDecoder(WicImagingFactory.CreateDecoderFromStream(stream, guidVendor, options));
        }

        public static WicBitmapDecoder Load(IStream stream, Guid? guidVendor = null, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            ArgumentNullException.ThrowIfNull(stream);

            return new WicBitmapDecoder(WicImagingFactory.CreateDecoderFromStream(stream, guidVendor, options));
        }

        public void Dispose() => _comObject.SafeDispose();
    }
}
