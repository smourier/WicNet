namespace WicNet.Utilities;

public class Dms(double degrees, double minutes, double seconds)
{
    public double Degrees => degrees;
    public double Minutes => minutes;
    public double Seconds => seconds;
    public double DecimalDegrees => (degrees * 3600 + minutes * 60 + seconds) / 3600;

    public override string ToString() => $"{degrees}° {minutes}' {seconds}\"";

    // https://learn.microsoft.com/en-us/windows/win32/properties/props-system-gps-latitude
    public static Dms? From(IReadOnlyList<ulong>? array) => TryFrom(array, out var value) ? value : null;

    public static bool TryFrom(IReadOnlyList<ulong>? array, [NotNullWhen(true)] out Dms? value)
    {
        value = null;
        if (array == null || array.Count != 3)
            return false;

        var degreesDenominator = (uint)(array[0] >> 32);
        var minutesDenominator = (uint)(array[1] >> 32);
        var secondsDenominator = (uint)(array[2] >> 32);
        if (degreesDenominator == 0 || minutesDenominator == 0 || secondsDenominator == 0)
            return false;

        var degrees = (uint)array[0] / (double)degreesDenominator;
        var minutes = (uint)array[1] / (double)minutesDenominator;
        var seconds = (uint)array[2] / (double)secondsDenominator;
        if (minutes >= 60 || seconds >= 60)
            return false;

        value = new Dms(degrees, minutes, seconds);
        return true;
    }
}
