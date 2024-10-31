using System;
using System.Collections.Generic;
using System.Linq;
using DirectN;

namespace WicNet
{
    public sealed class WicBitmapEncoder : IDisposable
    {
        private readonly IComObject<IWICBitmapEncoder> _comObject;

        public WicBitmapEncoder(object comObject)
        {
            _comObject = new ComObjectWrapper<IWICBitmapEncoder>(comObject).ComObject;
        }

        public IComObject<IWICBitmapEncoder> ComObject => _comObject;
        public Guid ContainerFormat => _comObject.GetContainerFormat();

        public WicMetadataQueryWriter GetMetadataQueryWriter()
        {
            var writer = _comObject.GetMetadataQueryWriter();
            return writer != null ? new WicMetadataQueryWriter(writer) : null;
        }

        public WICBitmapFrameEncode CreateNewFrame() => _comObject.CreateNewFrame();
        public void Commit() => _comObject.Commit();
        public void SetPreview(WicBitmapSource source) => _comObject.SetPreview(source.ComObject);
        public void SetThumbnail(WicBitmapSource source) => _comObject.SetThumbnail(source.ComObject);
        public void SetThumbnail(WicPalette palette) => _comObject.SetPalette(palette.ComObject);
        public void SetColorContexts(IEnumerable<WicColorContext> contexts)
        {
            if (contexts == null)
                throw new ArgumentNullException(nameof(contexts));

            _comObject.SetColorContexts(contexts.Select(c => c.ComObject.Object));
        }

        public static WicBitmapEncoder Load(Guid guidContainerFormat, Guid? guidVendor = null) => WICImagingFactory.WithFactory(f =>
        {
            // TODO: simplify on next DirectN version
            using (var guid = new ComMemory(guidVendor))
            {
                f.CreateEncoder(guidContainerFormat, guid.Pointer, out var encoder).ThrowOnError();
                return new WicBitmapEncoder(new ComObject<IWICBitmapEncoder>(encoder));
            }
        });

        public void Dispose() => _comObject.SafeDispose();
    }
}
