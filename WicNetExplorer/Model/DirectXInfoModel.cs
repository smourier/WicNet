using System;
using System.ComponentModel;
using DirectN;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DirectXInfoModel
    {
        public DirectXInfoModel(IComObject<ID2D1DeviceContext> context)
        {
            ArgumentNullException.ThrowIfNull(context);
            DeviceContext = new DeviceContextModel(context);
            Device = new DeviceModel(context.GetDevice());
        }

        [DisplayName("Device Context")]
        public DeviceContextModel DeviceContext { get; }

        public DeviceModel Device { get; }
    }
}
