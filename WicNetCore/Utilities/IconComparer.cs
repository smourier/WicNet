namespace WicNet.Utilities;

public class IconComparer : IComparer<WicBitmapSource>
{
    public virtual ListSortDirection Direction { get; set; }

    public virtual int Compare(WicBitmapSource? x, WicBitmapSource? y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        if (ReferenceEquals(x, y))
            return 0;

        var comp = DoCompare(x, y);
        return Direction == ListSortDirection.Ascending ? comp : -comp;
    }

    protected virtual uint GetColorCount(WicBitmapSource bmp)
    {
        ArgumentNullException.ThrowIfNull(bmp);

        if (bmp.Palette != null)
            return bmp.Palette.ColorCount;

        if (bmp.WicPixelFormat != null)
            return (uint)Math.Pow(2, bmp.WicPixelFormat.BitsPerPixel);

        return 0;
    }

    protected virtual int DoCompare(WicBitmapSource x, WicBitmapSource y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        var ix = x.IconInformation;
        var iy = y.IconInformation;
        if (ix != null && iy != null)
        {
            if (ix.ColorCount != iy.ColorCount)
                return ix.SortableColorCount.CompareTo(iy.SortableColorCount);
        }

        var xc = GetColorCount(x);
        var yc = GetColorCount(y);
        if (xc != yc)
            return xc.CompareTo(yc);

        var size = x.Width * x.Height;
        var otherSize = y.Width * y.Height;
        if (size != otherSize)
            return size.CompareTo(otherSize);

        if (ix == null || iy == null)
            return 0;

        // the lowest index the better, so need to reverse
        return -ix.Index.CompareTo(iy.Index);
    }
}
