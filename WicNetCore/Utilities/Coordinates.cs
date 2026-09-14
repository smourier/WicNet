namespace WicNet.Utilities;

public readonly struct Coordinates(double latitude, double longitude, double altitude)
{
    public double Latitude { get; } = latitude;
    public double Longitude { get; } = longitude;
    public double Altitude { get; } = altitude;

    public static Coordinates? Get(WicBitmapSource bitmap) => TryGet(bitmap, out var coordinates) ? coordinates : null;

    public static bool TryGet(WicBitmapSource bitmap, out Coordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        coordinates = default;
        using var reader = bitmap.GetMetadataReader();
        if (reader == null)
            return false;

        var obj = reader.GetMetadataByName<object>("/app1/{ushort=0}/{ushort=34853}");
        if (obj is not IWICMetadataQueryReader)
            return false;

        using var gpsReader = new ComObject<IWICMetadataQueryReader>(obj);
        using var r = new WicMetadataQueryReader(gpsReader);
        var latitudeRef = r.GetMetadataByName<string>("/{ushort=1}");
        var longitudeRef = r.GetMetadataByName<string>("/{ushort=3}");
        if (latitudeRef is not ("N" or "S") || longitudeRef is not ("E" or "W"))
            return false;
        if (!Dms.TryFrom(r.GetMetadataByName<IReadOnlyList<ulong>>("/{ushort=2}"), out var latitude) ||
            !Dms.TryFrom(r.GetMetadataByName<IReadOnlyList<ulong>>("/{ushort=4}"), out var longitude))
            return false;
        if (latitude.DecimalDegrees > 90 || longitude.DecimalDegrees > 180)
            return false;

        var altitudeArray = r.GetMetadataByName<IReadOnlyList<uint>>("/{ushort=6}");
        if (altitudeArray == null || altitudeArray.Count != 2 || altitudeArray[1] == 0)
            return false;
        if (!r.TryGetMetadataByName<byte>("/{ushort=5}", out var altitudeRef, out _) || altitudeRef > 1)
            return false;

        coordinates = new Coordinates(
            latitudeRef == "N" ? latitude.DecimalDegrees : -latitude.DecimalDegrees,
            longitudeRef == "E" ? longitude.DecimalDegrees : -longitude.DecimalDegrees,
            altitudeArray[0] / (double)altitudeArray[1] * (altitudeRef == 0 ? 1 : -1));
        return true;
    }
}
