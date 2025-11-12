namespace WicNet.Utilities;

public static class ID2D1ImageExtensions
{
    [SupportedOSPlatform("windows8.0")]
    public static void Save(this IComObject<ID2D1Image> image,
        IComObject<ID2D1Device> device,
        Guid encoderContainerFormat,
        Stream stream,
        WICImageParameters? parameters = null,
        Guid? pixelFormat = null,
        WICBitmapEncoderCacheOption cacheOptions = WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache,
        IEnumerable<KeyValuePair<string, object>>? encoderOptions = null,
        IEnumerable<WicMetadataKeyValue>? metadata = null,
        WicPalette? encoderPalette = null,
        WicPalette? framePalette = null,
        IEnumerable<WicColorContext>? colorContexts = null
        ) => (image?.Object!).Save(device?.Object!, encoderContainerFormat, stream, parameters, pixelFormat, cacheOptions, encoderOptions, metadata, encoderPalette, framePalette, colorContexts);

    [SupportedOSPlatform("windows8.0")]
    public static void Save(this ID2D1Image image,
        ID2D1Device device,
        Guid encoderContainerFormat,
        Stream stream,
        WICImageParameters? parameters = null,
        Guid? pixelFormat = null,
        WICBitmapEncoderCacheOption cacheOptions = WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache,
        IEnumerable<KeyValuePair<string, object>>? encoderOptions = null,
        IEnumerable<WicMetadataKeyValue>? metadata = null,
        WicPalette? encoderPalette = null,
        WicPalette? framePalette = null,
        IEnumerable<WicColorContext>? colorContexts = null
        )
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(stream);

        WicImagingFactory.WithFactory2(f =>
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
                    frame.SetColorContexts(colorContexts);
                }

                using (var imageEncoder = f.Object.CreateImageEncoder(device))
                {
                    if (parameters != null)
                    {
                        imageEncoder.Object.WriteFrame(image, frame.Encode.Object, parameters.Value).ThrowOnError();
                    }
                    else
                    {
                        imageEncoder.Object.WriteFrame(image, frame.Encode.Object, Unsafe.NullRef<WICImageParameters>()).ThrowOnError();
                    }
                }
                frame.Commit();
            }
            encoder.Commit();
        });
    }
}
