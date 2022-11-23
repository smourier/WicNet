using System;
using System.ComponentModel;
using DirectN;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DirectXInfoModel
    {
        public DirectXInfoModel(ID2DControl control, IComObject<ID2D1DeviceContext> context, IComObject<ID2D1Bitmap1>? bitmap)
        {
            ArgumentNullException.ThrowIfNull(control);
            ArgumentNullException.ThrowIfNull(context);
            DeviceContext = new DeviceContextModel(context);
            using var dev = context.GetDevice();
            Device = new DeviceModel(dev);
            Control = control.GetType().Name;
            Bitmap = bitmap != null ? new BitmapModel(bitmap) : null;
        }

        [DisplayName("Device Context")]
        public DeviceContextModel DeviceContext { get; }

        [DisplayName("D2D Control")]
        public string Control { get; }

        public BitmapModel? Bitmap { get; }
        public DeviceModel Device { get; }
    }
}
