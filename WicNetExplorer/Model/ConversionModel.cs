using System;
using WicNet;

namespace WicNetExplorer.Model;

public class ConversionModel(Guid from, Guid to)
{
    public PixelFormatModel From { get; } = new PixelFormatModel(WicPixelFormat.FromClsid(from));
    public PixelFormatModel To { get; } = new PixelFormatModel(WicPixelFormat.FromClsid(to));

    public override string ToString() => From + " => " + To;
}
