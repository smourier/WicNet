namespace WicNet;

public sealed class WicMetadataWriter(IComObject<IWICMetadataWriterInfo> comObject) : WicMetadataHandler(comObject)
{
}
