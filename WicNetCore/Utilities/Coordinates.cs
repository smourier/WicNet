namespace WicNet.Utilities;

public readonly struct Coordinates(double latitude, double longitude, double altitude)
{
    public double Latitude { get; } = latitude;
    public double Longitude { get; } = longitude;
    public double Altitude { get; } = altitude;

    public static Coordinates? Get(WicBitmapSource bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        using var reader = bitmap.GetMetadataReader()!;

        // get GPS metadata reader
        var obj = reader.GetMetadataByName<object>("/app1/{ushort=0}/{ushort=34853}");
        if (obj is not IWICMetadataQueryReader)
            return null;

        using var gpsReader = new ComObject<IWICMetadataQueryReader>(obj);
        // https://learn.microsoft.com/en-us/windows/win32/wic/-wic-native-image-format-metadata-queries#gps-metadata
        using var r = new WicMetadataQueryReader(gpsReader);

        // https://learn.microsoft.com/en-us/windows/win32/wic/-wic-photoprop-system-gps-altituderef
        var altitudeRef = r.GetMetadataByName<byte>("/{ushort=5}") == 0 ? 1 : -1;
        var altitudeArray = r.GetMetadataByName<IReadOnlyList<uint>>("/{ushort=6}")!;
        var altitude = altitudeArray[0] / (double)altitudeArray[1] * altitudeRef;

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // force '.' as decimal separator

        var latitudeRef = r.GetMetadataByName<string>("/{ushort=1}");
        var latitudeArray = r.GetMetadataByName<IReadOnlyList<ulong>>("/{ushort=2}")!;
        var longitudeRef = r.GetMetadataByName<string>("/{ushort=3}");
        var longitudeArray = r.GetMetadataByName<IReadOnlyList<ulong>>("/{ushort=4}")!;

        var latitude = Dms.From(latitudeArray);
        var longitude = Dms.From(longitudeArray);
        return new Coordinates(
            latitudeRef == "N" ? latitude.DecimalDegrees : -latitude.DecimalDegrees,
            longitudeRef == "E" ? longitude.DecimalDegrees : -longitude.DecimalDegrees,
            altitude
        );
    }
}
