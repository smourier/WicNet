namespace WicNet.Utilities;

public sealed class IconInformation
{
    internal IconInformation()
    {
    }

    public string Id { get; internal set; } = null!;
    public int Index { get; internal set; }
    public string GroupId { get; internal set; } = null!;
    public int GroupIndex { get; internal set; }
    public int ColorCount { get; internal set; }

    internal int SortableColorCount => ColorCount != 0 ? ColorCount : int.MaxValue;

    public override string ToString() => $"{Id} [{Index}] ({ColorCount} colors) {GroupId} [{GroupIndex}]";
}
