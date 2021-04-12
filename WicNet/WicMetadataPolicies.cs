using System;
using System.Collections.Generic;

namespace WicNet
{
    public class WicMetadataPolicies
    {
        public WicMetadataPolicies(WicMetadataQueryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Reader = reader;
        }

        public WicMetadataQueryReader Reader { get; }

        public string ImageId => Reader.GetMetadataByName<string>("System.Image.ImageID");
        public string ImageDimensions => Reader.GetMetadataByName<string>("System.Image.Dimensions");
        public double? ImageHorizontalResolution => Reader.GetMetadataByName<double?>("System.Image.HorizontalResolution");
        public double? ImageVerticalResolution => Reader.GetMetadataByName<double?>("System.Image.VerticalResolution");
        public double? ImageResolutionUnit => Reader.GetMetadataByName<double?>("System.Image.ResolutionUnit");
        public ushort? ImageColorSpace => Reader.GetMetadataByName<ushort?>("System.Image.ColorSpace");
        public ushort? ImageCompression => Reader.GetMetadataByName<ushort?>("System.Image.Compression");
        public uint? ImageBitDepth => Reader.GetMetadataByName<uint?>("System.Image.BitDepth");
        public uint? ImageVerticalSize => Reader.GetMetadataByName<uint?>("System.Image.VerticalSize");
        public uint? ImageHorizontalSize => Reader.GetMetadataByName<uint?>("System.Image.HorizontalSize");
        public uint? ImageCompressedBitsPerPixelNumerator => Reader.GetMetadataByName<uint?>("System.Image.CompressedBitsPerPixelNumerator");
        public uint? ImageCompressedBitsPerPixelDenominator => Reader.GetMetadataByName<uint?>("System.Image.CompressedBitsPerPixelDenominator");
        public double? ImageCompressedBitsPerPixel => Reader.GetMetadataByName<double?>("System.Image.CompressedBitsPerPixel");

        public string ApplicationName => Reader.GetMetadataByName<string>("System.ApplicationName");
        public string Author => Reader.GetMetadataByName<string>("System.Author");
        public string Comment => Reader.GetMetadataByName<string>("System.Comment");
        public string Copyright => Reader.GetMetadataByName<string>("System.Copyright");
        public DateTime? DateAcquired => Reader.GetMetadataByName<DateTime?>("System.DateAcquired");
        public IReadOnlyList<string> Keywords => Reader.GetMetadataByName<IReadOnlyList<string>>("System.Keywords");
        public uint? Rating => Reader.GetMetadataByName<uint?>("System.Rating");
        public int? SimpleRating => Reader.GetMetadataByName<int?>("System.SimpleRating");
        public string Subject => Reader.GetMetadataByName<string>("System.Subject");
        public string Title => Reader.GetMetadataByName<string>("System.Title");

        public double? GPSAltitude => Reader.GetMetadataByName<double?>("System.GPS.Altitude");
        public byte? GPSAltitudeRef => Reader.GetMetadataByName<byte?>("System.GPS.AltitudeRef");
        public string GPSAreaInformation => Reader.GetMetadataByName<string>("System.GPS.AreaInformation");
        public double? GPSDestBearing => Reader.GetMetadataByName<double?>("System.GPS.DestBearing");
        public string GPSDestBearingRef => Reader.GetMetadataByName<string>("System.GPS.DestBearingRef");
        public double? GPSDestDistance => Reader.GetMetadataByName<double?>("System.GPS.DestDistance");
        public string GPSDestDistanceRef => Reader.GetMetadataByName<string>("System.GPS.DestDistanceRef");
        public IReadOnlyList<double> GPSDestLatitude => Reader.GetMetadataByName<IReadOnlyList<double>>("System.GPS.DestLatitude");
        public string GPSDestLatitudeRef => Reader.GetMetadataByName<string>("System.GPS.DestLatitudeRef");
        public IReadOnlyList<double> GPSDestLongitude => Reader.GetMetadataByName<IReadOnlyList<double>>("System.GPS.DestLongitude");
        public string GPSDestLongitudeRef => Reader.GetMetadataByName<string>("System.GPS.DestLongitudeRef");
        public ushort? GPSDifferential => Reader.GetMetadataByName<ushort?>("System.GPS.Differential");
        public double? GPSDOP => Reader.GetMetadataByName<double?>("System.GPS.DOP");
        public double? GPSImgDirection => Reader.GetMetadataByName<double?>("System.GPS.ImgDirection");
        public string GPSImgDirectionRef => Reader.GetMetadataByName<string>("System.GPS.ImgDirectionRef");
        public IReadOnlyList<double> GPSLatitude => Reader.GetMetadataByName<IReadOnlyList<double>>("System.GPS.Latitude");
        public string GPSLatitudeRef => Reader.GetMetadataByName<string>("System.GPS.LatitudeRef");
        public IReadOnlyList<double> GPSLongitude => Reader.GetMetadataByName<IReadOnlyList<double>>("System.GPS.Longitude");
        public string GPSLongitudeRef => Reader.GetMetadataByName<string>("System.GPS.LongitudeRef");
        public string GPSMapDatum => Reader.GetMetadataByName<string>("System.GPS.MapDatum");
        public string GPSMeasureMode => Reader.GetMetadataByName<string>("System.GPS.MeasureMode");
        public string GPSProcessingMethod => Reader.GetMetadataByName<string>("System.GPS.ProcessingMethod");
        public string GPSSatellites => Reader.GetMetadataByName<string>("System.GPS.Satellites");
        public double? GPSSpeed => Reader.GetMetadataByName<double?>("System.GPS.Speed");
        public string GPSSpeedRef => Reader.GetMetadataByName<string>("System.GPS.SpeedRef");
        public string GPSStatus => Reader.GetMetadataByName<string>("System.GPS.Status");
        public double? GPSTrack => Reader.GetMetadataByName<double?>("System.GPS.Track");
        public string GPSTrackRef => Reader.GetMetadataByName<string>("System.GPS.TrackRef");
        public byte[] GPSVersionID => Reader.GetMetadataByName<byte[]>("System.GPS.VersionID");

        public double? PhotoAperture => Reader.GetMetadataByName<double?>("System.Photo.Aperture");
        public double? PhotoBrightness => Reader.GetMetadataByName<double?>("System.Photo.Brightness");
        public string PhotoCameraManufacturer => Reader.GetMetadataByName<string>("System.Photo.CameraManufacturer");
        public string PhotoCameraModel => Reader.GetMetadataByName<string>("System.Photo.CameraModel");
        public string PhotoCameraSerialNumber => Reader.GetMetadataByName<string>("System.Photo.CameraSerialNumber");
        public uint? PhotoContrast => Reader.GetMetadataByName<uint?>("System.Photo.Contrast");
        public DateTime? PhotoDateTaken => Reader.GetMetadataByName<DateTime?>("System.Photo.DateTaken");
        public double? PhotoDigitalZoom => Reader.GetMetadataByName<double?>("System.Photo.DigitalZoom");
        public string PhotoEXIFVersion => Reader.GetMetadataByName<string>("System.Photo.EXIFVersion");
        public double? PhotoExposureBias => Reader.GetMetadataByName<double?>("System.Photo.ExposureBias");
        public double? PhotoExposureTime => Reader.GetMetadataByName<double?>("System.Photo.ExposureTime");
        public uint? PhotoFlash => Reader.GetMetadataByName<uint?>("System.Photo.Flash");
        public double? PhotoFlashEnergy => Reader.GetMetadataByName<double?>("System.Photo.FlashEnergy");
        public string PhotoFlashManufacturer => Reader.GetMetadataByName<string>("System.Photo.FlashManufacturer");
        public string PhotoFlashModel => Reader.GetMetadataByName<string>("System.Photo.FlashModel");
        public double? PhotoFNumber => Reader.GetMetadataByName<double?>("System.Photo.FNumber");
        public double? PhotoFocalLength => Reader.GetMetadataByName<double?>("System.Photo.FocalLength");
        public ushort? PhotoFocalLengthInFilm => Reader.GetMetadataByName<ushort?>("System.Photo.FocalLengthInFilm");
        public ushort? PhotoISOSpeed => Reader.GetMetadataByName<ushort?>("System.Photo.ISOSpeed");
        public string PhotoLensManufacturer => Reader.GetMetadataByName<string>("System.Photo.LensManufacturer");
        public string PhotoLensModel => Reader.GetMetadataByName<string>("System.Photo.LensModel");
        public uint? PhotoLightSource => Reader.GetMetadataByName<uint?>("System.Photo.LightSource");
        public byte[] PhotoMakerNote => Reader.GetMetadataByName<byte[]>("System.Photo.MakerNote");
        public double? PhotoMaxAperture => Reader.GetMetadataByName<double?>("System.Photo.MaxAperture");
        public ushort? PhotoMeteringMode => Reader.GetMetadataByName<ushort?>("System.Photo.MeteringMode");
        public ushort? PhotoOrientation => Reader.GetMetadataByName<ushort?>("System.Photo.Orientation");
        public IReadOnlyList<string> PhotoPeopleNames => Reader.GetMetadataByName<IReadOnlyList<string>>("System.Photo.PeopleNames");
        public ushort? PhotoPhotometricInterpretation => Reader.GetMetadataByName<ushort?>("System.Photo.PhotometricInterpretation");
        public uint? PhotoProgramMode => Reader.GetMetadataByName<uint?>("System.Photo.ProgramMode");
        public string PhotoRelatedSoundFile => Reader.GetMetadataByName<string>("System.Photo.RelatedSoundFile");
        public uint? PhotoSaturation => Reader.GetMetadataByName<uint?>("System.Photo.Saturation");
        public uint? PhotoSharpness => Reader.GetMetadataByName<uint?>("System.Photo.Sharpness");
        public double? PhotoShutterSpeed => Reader.GetMetadataByName<double?>("System.Photo.ShutterSpeed");
        public bool? PhotoTranscodedForSync => Reader.GetMetadataByName<bool?>("System.Photo.TranscodedForSync");
        public uint? PhotoPhotoWhiteBalance => Reader.GetMetadataByName<uint?>("System.Photo.WhiteBalance");
    }
}
