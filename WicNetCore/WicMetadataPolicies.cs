namespace WicNet;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class WicMetadataPolicies
{
    public WicMetadataPolicies(WicMetadataQueryReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        Reader = reader;
    }

    [Browsable(false)]
    public WicMetadataQueryReader Reader { get; }

    [Browsable(false)]
    public int FilledValues
    {
        get
        {
            var count = 0;
            foreach (var prop in GetType().GetProperties())
            {
                var dna = prop.CustomAttributes.Where(a => a.AttributeType.FullName == typeof(DisplayNameAttribute).FullName).FirstOrDefault();
                if (dna == null)
                    continue;

                var dn = dna.ConstructorArguments.FirstOrDefault().Value as string;
                if (dn == null || dn.IndexOf('.') < 0)
                    continue;

                try
                {
                    var value = prop.GetValue(this);
                    if (value == null)
                        continue;

                    count++;
                }
                catch
                {
                    // continue;
                }
            }
            return count;
        }
    }

    [DisplayName("System.Image.ImageID")] public string? ImageId => Reader.GetMetadataByName<string>("System.Image.ImageID");
    [DisplayName("System.Image.Dimensions")] public string? ImageDimensions => Reader.GetMetadataByName<string>("System.Image.Dimensions");
    [DisplayName("System.Image.HorizontalResolution")] public double? ImageHorizontalResolution => Reader.GetMetadataByName<double?>("System.Image.HorizontalResolution");
    [DisplayName("System.Image.VerticalResolution")] public double? ImageVerticalResolution => Reader.GetMetadataByName<double?>("System.Image.VerticalResolution");
    [DisplayName("System.Image.ResolutionUnit")] public double? ImageResolutionUnit => Reader.GetMetadataByName<double?>("System.Image.ResolutionUnit");
    [DisplayName("System.Image.ColorSpace")] public ushort? ImageColorSpace => Reader.GetMetadataByName<ushort?>("System.Image.ColorSpace");
    [DisplayName("System.Image.Compression")] public ushort? ImageCompression => Reader.GetMetadataByName<ushort?>("System.Image.Compression");
    [DisplayName("System.Image.BitDepth")] public uint? ImageBitDepth => Reader.GetMetadataByName<uint?>("System.Image.BitDepth");
    [DisplayName("System.Image.VerticalSize")] public uint? ImageVerticalSize => Reader.GetMetadataByName<uint?>("System.Image.VerticalSize");
    [DisplayName("System.Image.HorizontalSize")] public uint? ImageHorizontalSize => Reader.GetMetadataByName<uint?>("System.Image.HorizontalSize");
    [DisplayName("System.Image.CompressedBitsPerPixelNumerator")] public uint? ImageCompressedBitsPerPixelNumerator => Reader.GetMetadataByName<uint?>("System.Image.CompressedBitsPerPixelNumerator");
    [DisplayName("System.Image.CompressedBitsPerPixelDenominator")] public uint? ImageCompressedBitsPerPixelDenominator => Reader.GetMetadataByName<uint?>("System.Image.CompressedBitsPerPixelDenominator");
    [DisplayName("System.Image.CompressedBitsPerPixel")] public double? ImageCompressedBitsPerPixel => Reader.GetMetadataByName<double?>("System.Image.CompressedBitsPerPixel");

    [DisplayName("System.ApplicationName")] public string? ApplicationName => Reader.GetMetadataByName<string>("System.ApplicationName");
    [DisplayName("System.Author")] public IReadOnlyList<string> Author => Reader.GetMetadataByName<IReadOnlyList<string>>("System.Author") ?? [];
    [DisplayName("System.Comment")] public string? Comment => Reader.GetMetadataByName<string>("System.Comment");
    [DisplayName("System.Copyright")] public string? Copyright => Reader.GetMetadataByName<string>("System.Copyright");
    [DisplayName("System.DateAcquired")] public DateTime? DateAcquired => Reader.GetMetadataByName<DateTime?>("System.DateAcquired");
    [DisplayName("System.Keywords")] public IReadOnlyList<string> Keywords => Reader.GetMetadataByName<IReadOnlyList<string>>("System.Keywords") ?? [];
    [DisplayName("System.Rating")] public uint? Rating => Reader.GetMetadataByName<uint?>("System.Rating");
    [DisplayName("System.SimpleRating")] public int? SimpleRating => Reader.GetMetadataByName<int?>("System.SimpleRating");
    [DisplayName("System.Subject")] public string? Subject => Reader.GetMetadataByName<string>("System.Subject");
    [DisplayName("System.Title")] public string? Title => Reader.GetMetadataByName<string>("System.Title");

    [DisplayName("System.GPS.Altitude")] public double? GPSAltitude => Reader.GetMetadataByName<double?>("System.GPS.Altitude");
    [DisplayName("System.GPS.AltitudeRef")] public byte? GPSAltitudeRef => Reader.GetMetadataByName<byte?>("System.GPS.AltitudeRef");
    [DisplayName("System.GPS.AreaInformation")] public string? GPSAreaInformation => Reader.GetMetadataByName<string>("System.GPS.AreaInformation");
    [DisplayName("System.GPS.DestBearing")] public double? GPSDestBearing => Reader.GetMetadataByName<double?>("System.GPS.DestBearing");
    [DisplayName("System.GPS.DestBearingRef")] public string? GPSDestBearingRef => Reader.GetMetadataByName<string>("System.GPS.DestBearingRef");
    [DisplayName("System.GPS.DestDistance")] public double? GPSDestDistance => Reader.GetMetadataByName<double?>("System.GPS.DestDistance");
    [DisplayName("System.GPS.DestDistanceRef")] public string? GPSDestDistanceRef => Reader.GetMetadataByName<string>("System.GPS.DestDistanceRef");
    [DisplayName("System.GPS.DestLatitude")] public IReadOnlyList<double> GPSDestLatitude => Reader.GetMetadataByName<IReadOnlyList<double>>("System.GPS.DestLatitude") ?? [];
    [DisplayName("System.GPS.DestLatitudeRef")] public string? GPSDestLatitudeRef => Reader.GetMetadataByName<string>("System.GPS.DestLatitudeRef");
    [DisplayName("System.GPS.DestLongitude")] public IReadOnlyList<double> GPSDestLongitude => Reader.GetMetadataByName<IReadOnlyList<double>>("System.GPS.DestLongitude") ?? [];
    [DisplayName("System.GPS.DestLongitudeRef")] public string? GPSDestLongitudeRef => Reader.GetMetadataByName<string>("System.GPS.DestLongitudeRef");
    [DisplayName("System.GPS.Differential")] public ushort? GPSDifferential => Reader.GetMetadataByName<ushort?>("System.GPS.Differential");
    [DisplayName("System.GPS.DOP")] public double? GPSDOP => Reader.GetMetadataByName<double?>("System.GPS.DOP");
    [DisplayName("System.GPS.ImgDirection")] public double? GPSImgDirection => Reader.GetMetadataByName<double?>("System.GPS.ImgDirection");
    [DisplayName("System.GPS.ImgDirectionRef")] public string? GPSImgDirectionRef => Reader.GetMetadataByName<string>("System.GPS.ImgDirectionRef");
    [DisplayName("System.GPS.Latitude")] public IReadOnlyList<ulong> GPSLatitude => Reader.GetMetadataByName<IReadOnlyList<ulong>>("System.GPS.Latitude") ?? [];
    [DisplayName("System.GPS.LatitudeDecimal")] public double GPSLatitudeDecimal => Reader.GetMetadataByName<double>("System.GPS.LatitudeDecimal");
    [DisplayName("System.GPS.LatitudeRef")] public string? GPSLatitudeRef => Reader.GetMetadataByName<string>("System.GPS.LatitudeRef");
    [DisplayName("System.GPS.LatitudeDenominator")] public IReadOnlyList<uint> GPSLatitudeDenominator => Reader.GetMetadataByName<IReadOnlyList<uint>>("System.GPS.LatitudeDenominator") ?? [];
    [DisplayName("System.GPS.LatitudeNumerator")] public IReadOnlyList<uint> GPSLatitudeNumerator => Reader.GetMetadataByName<IReadOnlyList<uint>>("System.GPS.LatitudeNumerator") ?? [];
    [DisplayName("System.GPS.Longitude")] public IReadOnlyList<ulong> GPSLongitude => Reader.GetMetadataByName<IReadOnlyList<ulong>>("System.GPS.Longitude") ?? [];
    [DisplayName("System.GPS.LongitudeDecimal")] public double GPSLongitudeDecimal => Reader.GetMetadataByName<double>("System.GPS.LongitudeDecimal");
    [DisplayName("System.GPS.LongitudeRef")] public string? GPSLongitudeRef => Reader.GetMetadataByName<string>("System.GPS.LongitudeRef");
    [DisplayName("System.GPS.LongitudeDenominator")] public IReadOnlyList<uint> GPSLongitudeDenominator => Reader.GetMetadataByName<IReadOnlyList<uint>>("System.GPS.LongitudeDenominator") ?? [];
    [DisplayName("System.GPS.LongitudeNumerator")] public IReadOnlyList<uint> GPSLongitudeNumerator => Reader.GetMetadataByName<IReadOnlyList<uint>>("System.GPS.LongitudeNumerator") ?? [];
    [DisplayName("System.GPS.MapDatum")] public string? GPSMapDatum => Reader.GetMetadataByName<string>("System.GPS.MapDatum");
    [DisplayName("System.GPS.MeasureMode")] public string? GPSMeasureMode => Reader.GetMetadataByName<string>("System.GPS.MeasureMode");
    [DisplayName("System.GPS.ProcessingMethod")] public string? GPSProcessingMethod => Reader.GetMetadataByName<string>("System.GPS.ProcessingMethod");
    [DisplayName("System.GPS.Satellites")] public string? GPSSatellites => Reader.GetMetadataByName<string>("System.GPS.Satellites");
    [DisplayName("System.GPS.Speed")] public double? GPSSpeed => Reader.GetMetadataByName<double?>("System.GPS.Speed");
    [DisplayName("System.GPS.SpeedRef")] public string? GPSSpeedRef => Reader.GetMetadataByName<string>("System.GPS.SpeedRef");
    [DisplayName("System.GPS.Status")] public string? GPSStatus => Reader.GetMetadataByName<string>("System.GPS.Status");
    [DisplayName("System.GPS.Track")] public double? GPSTrack => Reader.GetMetadataByName<double?>("System.GPS.Track");
    [DisplayName("System.GPS.TrackRef")] public string? GPSTrackRef => Reader.GetMetadataByName<string>("System.GPS.TrackRef");
    [DisplayName("System.GPS.VersionID")] public byte[]? GPSVersionID => Reader.GetMetadataByName<byte[]>("System.GPS.VersionID");

    [DisplayName("System.Photo.Aperture")] public double? PhotoAperture => Reader.GetMetadataByName<double?>("System.Photo.Aperture");
    [DisplayName("System.Photo.Brightness")] public double? PhotoBrightness => Reader.GetMetadataByName<double?>("System.Photo.Brightness");
    [DisplayName("System.Photo.CameraManufacturer")] public string? PhotoCameraManufacturer => Reader.GetMetadataByName<string>("System.Photo.CameraManufacturer");
    [DisplayName("System.Photo.CameraModel")] public string? PhotoCameraModel => Reader.GetMetadataByName<string>("System.Photo.CameraModel");
    [DisplayName("System.Photo.CameraSerialNumber")] public string? PhotoCameraSerialNumber => Reader.GetMetadataByName<string>("System.Photo.CameraSerialNumber");
    [DisplayName("System.Photo.Contrast")] public uint? PhotoContrast => Reader.GetMetadataByName<uint?>("System.Photo.Contrast");
    [DisplayName("System.Photo.DateTaken")] public DateTime? PhotoDateTaken => Reader.GetMetadataByName<DateTime?>("System.Photo.DateTaken");
    [DisplayName("System.Photo.DigitalZoom")] public double? PhotoDigitalZoom => Reader.GetMetadataByName<double?>("System.Photo.DigitalZoom");
    [DisplayName("System.Photo.EXIFVersion")] public string? PhotoEXIFVersion => Reader.GetMetadataByName<string>("System.Photo.EXIFVersion");
    [DisplayName("System.Photo.ExposureBias")] public double? PhotoExposureBias => Reader.GetMetadataByName<double?>("System.Photo.ExposureBias");
    [DisplayName("System.Photo.ExposureTime")] public double? PhotoExposureTime => Reader.GetMetadataByName<double?>("System.Photo.ExposureTime");
    [DisplayName("System.Photo.Flash")] public uint? PhotoFlash => Reader.GetMetadataByName<uint?>("System.Photo.Flash");
    [DisplayName("System.Photo.FlashEnergy")] public double? PhotoFlashEnergy => Reader.GetMetadataByName<double?>("System.Photo.FlashEnergy");
    [DisplayName("System.Photo.FlashManufacturer")] public string? PhotoFlashManufacturer => Reader.GetMetadataByName<string>("System.Photo.FlashManufacturer");
    [DisplayName("System.Photo.FlashModel")] public string? PhotoFlashModel => Reader.GetMetadataByName<string>("System.Photo.FlashModel");
    [DisplayName("System.Photo.FNumber")] public double? PhotoFNumber => Reader.GetMetadataByName<double?>("System.Photo.FNumber");
    [DisplayName("System.Photo.FocalLength")] public double? PhotoFocalLength => Reader.GetMetadataByName<double?>("System.Photo.FocalLength");
    [DisplayName("System.Photo.FocalLengthInFilm")] public ushort? PhotoFocalLengthInFilm => Reader.GetMetadataByName<ushort?>("System.Photo.FocalLengthInFilm");
    [DisplayName("System.Photo.ISOSpeed")] public ushort? PhotoISOSpeed => Reader.GetMetadataByName<ushort?>("System.Photo.ISOSpeed");
    [DisplayName("System.Photo.LensManufacturer")] public string? PhotoLensManufacturer => Reader.GetMetadataByName<string>("System.Photo.LensManufacturer");
    [DisplayName("System.Photo.LensModel")] public string? PhotoLensModel => Reader.GetMetadataByName<string>("System.Photo.LensModel");
    [DisplayName("System.Photo.LightSource")] public uint? PhotoLightSource => Reader.GetMetadataByName<uint?>("System.Photo.LightSource");
    [DisplayName("System.Photo.MakerNote")] public byte[]? PhotoMakerNote => Reader.GetMetadataByName<byte[]>("System.Photo.MakerNote");
    [DisplayName("System.Photo.MaxAperture")] public double? PhotoMaxAperture => Reader.GetMetadataByName<double?>("System.Photo.MaxAperture");
    [DisplayName("System.Photo.MeteringMode")] public ushort? PhotoMeteringMode => Reader.GetMetadataByName<ushort?>("System.Photo.MeteringMode");
    [DisplayName("System.Photo.Orientation")] public ushort? PhotoOrientation => Reader.GetMetadataByName<ushort?>("System.Photo.Orientation");
    [DisplayName("System.Photo.PeopleNames")] public IReadOnlyList<string> PhotoPeopleNames => Reader.GetMetadataByName<IReadOnlyList<string>>("System.Photo.PeopleNames") ?? [];
    [DisplayName("System.Photo.PhotometricInterpretation")] public ushort? PhotoPhotometricInterpretation => Reader.GetMetadataByName<ushort?>("System.Photo.PhotometricInterpretation");
    [DisplayName("System.Photo.ProgramMode")] public uint? PhotoProgramMode => Reader.GetMetadataByName<uint?>("System.Photo.ProgramMode");
    [DisplayName("System.Photo.RelatedSoundFile")] public string? PhotoRelatedSoundFile => Reader.GetMetadataByName<string>("System.Photo.RelatedSoundFile");
    [DisplayName("System.Photo.Saturation")] public uint? PhotoSaturation => Reader.GetMetadataByName<uint?>("System.Photo.Saturation");
    [DisplayName("System.Photo.Sharpness")] public uint? PhotoSharpness => Reader.GetMetadataByName<uint?>("System.Photo.Sharpness");
    [DisplayName("System.Photo.ShutterSpeed")] public double? PhotoShutterSpeed => Reader.GetMetadataByName<double?>("System.Photo.ShutterSpeed");
    [DisplayName("System.Photo.TranscodedForSync")] public bool? PhotoTranscodedForSync => Reader.GetMetadataByName<bool?>("System.Photo.TranscodedForSync");
    [DisplayName("System.Photo.WhiteBalance")] public uint? PhotoPhotoWhiteBalance => Reader.GetMetadataByName<uint?>("System.Photo.WhiteBalance");

    public override string ToString() => string.Empty;
}
