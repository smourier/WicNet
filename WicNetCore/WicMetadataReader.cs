namespace WicNet;

public sealed class WicMetadataReader(IComObject<IWICMetadataReaderInfo> comObject) : WicMetadataHandler(comObject)
{
}
