using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DirectN;

namespace WicNet.Utilities;

public static class ID2D1ImageExtensions
{
    public static void Save(this IComObject<ID2D1Image> image,
        IComObject<ID2D1Device> device,
        Guid encoderContainerFormat,
        Stream stream,
        WICImageParameters? parameters = null,
        Guid? pixelFormat = null,
        WICBitmapEncoderCacheOption cacheOptions = WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache,
        IEnumerable<KeyValuePair<string, object>> encoderOptions = null,
        IEnumerable<WicMetadataKeyValue> metadata = null,
        WicPalette encoderPalette = null,
        WicPalette framePalette = null,
        IEnumerable<WicColorContext> colorContexts = null
        ) => (image?.Object).Save(device?.Object, encoderContainerFormat, stream, parameters, pixelFormat, cacheOptions, encoderOptions, metadata, encoderPalette, framePalette, colorContexts);

    public static void Save(this ID2D1Image image,
        ID2D1Device device,
        Guid encoderContainerFormat,
        Stream stream,
        WICImageParameters? parameters = null,
        Guid? pixelFormat = null,
        WICBitmapEncoderCacheOption cacheOptions = WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache,
        IEnumerable<KeyValuePair<string, object>> encoderOptions = null,
        IEnumerable<WicMetadataKeyValue> metadata = null,
        WicPalette encoderPalette = null,
        WicPalette framePalette = null,
        IEnumerable<WicColorContext> colorContexts = null
        )
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image));

        if (device == null)
            throw new ArgumentNullException(nameof(device));

        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        WICImagingFactory.WithFactory2(f =>
        {
            using var encoder = f.CreateEncoder(encoderContainerFormat);
            var mis = new ManagedIStream(stream);
            encoder.Initialize(mis, cacheOptions);

            if (encoderPalette != null)
            {
                // gifs...
                encoder.SetPalette(encoderPalette.ComObject);
            }

            using (var frame = encoder.CreateNewFrame())
            {
                if (encoderOptions != null)
                {
                    frame.Bag.Write(encoderOptions);
                }

                frame.Initialize();

                if (metadata?.Any() == true)
                {
                    using var writer = frame.GetMetadataQueryWriter();
                    writer.EncodeMetadata(metadata);
                }

                if (pixelFormat.HasValue)
                {
                    frame.SetPixelFormat(pixelFormat.Value);
                }

                if (framePalette != null)
                {
                    frame.Encode.SetPalette(framePalette.ComObject);
                }

                if (colorContexts?.Any() == true)
                {
                    frame.SetColorContexts(colorContexts.Select(c => c.ComObject.Object));
                }

                using (var imageEncoder = f.CreateImageEncoder(device))
                {
                    using var mem = parameters.StructureToMemory();
                    imageEncoder.Object.WriteFrame(image, frame.Encode.Object, mem.Pointer).ThrowOnError();
                }
                frame.Commit();
            }
            encoder.Commit();
        });
    }
}
