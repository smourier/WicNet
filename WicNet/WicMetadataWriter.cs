using DirectN;

namespace WicNet
{
    public sealed class WicMetadataWriter : WicMetadataHandler
    {
        public WicMetadataWriter(object comObject)
            : base(comObject)
        {
            using (var info = new ComObjectWrapper<IWICMetadataWriterInfo>(comObject))
            {
                // nothing here today
            }
        }
    }
}
