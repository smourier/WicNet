using System;
using System.Globalization;

namespace WicNet;

[Serializable]
public class WicNetException : Exception
{
    public WicNetException()
        : base("Wic Exception")
    {
    }

    public WicNetException(string message)
        : base(message)
    {
    }

    public WicNetException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public WicNetException(Exception innerException)
        : base(null, innerException)
    {
    }

    public static int GetCode(string message)
    {
        if (message == null)
            return -1;

        const string prefix = "WIC";
        if (!message.StartsWith(prefix, StringComparison.Ordinal))
            return -1;

        var pos = message.IndexOf(':', prefix.Length);
        if (pos < 0)
            return -1;

        if (int.TryParse(message.AsSpan(prefix.Length, pos - prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var i))
            return i;

        return -1;
    }

    public int Code => GetCode(Message);
}
