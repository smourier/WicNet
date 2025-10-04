namespace WicNet.Utilities;

public class Dms(double degrees, double minutes, double seconds)
{
    public double Degrees => degrees;
    public double Minutes => minutes;
    public double Seconds => seconds;
    public double DecimalDegrees => (degrees * 3600 + minutes * 60 + seconds) / 3600;

    public override string ToString() => $"{degrees}° {minutes}' {seconds}\"";

    // https://learn.microsoft.com/en-us/windows/win32/properties/props-system-gps-latitude
    public static Dms From(IReadOnlyList<ulong> array)
    {
        var degreesArray = WicMetadataQueryReader.ChangeType<IReadOnlyList<uint>>(array[0])!;
        var minutesArray = WicMetadataQueryReader.ChangeType<IReadOnlyList<uint>>(array[1])!;
        var secondsArray = WicMetadataQueryReader.ChangeType<IReadOnlyList<uint>>(array[2])!;
        var degrees = degreesArray[0] / (double)degreesArray[1];
        var minutes = minutesArray[0] / (double)minutesArray[1];
        var seconds = secondsArray[0] / (double)secondsArray[1];
        return new Dms(degrees, minutes, seconds);
    }
}