using WicNet.Interop;

namespace WicNet
{
    public class WicMetadataWriter : WicMetadataHandler
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
