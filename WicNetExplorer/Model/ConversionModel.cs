using System;
using WicNet;

namespace WicNetExplorer.Model
{
    public class ConversionModel
    {
        public ConversionModel(Guid from, Guid to)
        {
            From = new PixelFormatModel(WicPixelFormat.FromClsid(from));
            To = new PixelFormatModel(WicPixelFormat.FromClsid(to));
        }

        public PixelFormatModel From { get; }
        public PixelFormatModel To { get; }

        public override string ToString() => From + " => " + To;
    }
}
