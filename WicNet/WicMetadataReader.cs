using WicNet.Interop;

namespace WicNet
{
    public class WicMetadataReader : WicMetadataHandler
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
