using System;
using WicNet;

namespace WicNetExplorer.Model;

public class ConversionModel(Guid from, Guid to)
{
    public PixelFormatModel? From { get; } = PixelFormatModel.From(WicPixelFormat.FromClsid(from));
    public PixelFormatModel? To { get; } = PixelFormatModel.From(WicPixelFormat.FromClsid(to));

    public override string ToString() => From + " => " + To;
}
