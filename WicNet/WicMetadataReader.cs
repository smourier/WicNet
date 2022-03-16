using DirectN;

namespace WicNet
{
    public sealed class WicMetadataReader : WicMetadataHandler
    {
        public WicMetadataReader(object comObject)
            : base(comObject)
        {
            using (var info = new ComObjectWrapper<IWICMetadataReaderInfo>(comObject))
            {
                // nothing here today
            }
        }
    }
}
