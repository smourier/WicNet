using System;
using System.ComponentModel;
using DirectN;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DeviceModel
    {
        private readonly string _text;

        public DeviceModel(IComObject<ID2D1Device> device)
        {
            ArgumentNullException.ThrowIfNull(device);
            _text = device.InterfaceType.Name;
            RenderingPriority = (device.As<ID2D1Device1>()?.GetRenderingPriority()).GetValueOrDefault();
            MaximumTextureMemory = device.Object.GetMaximumTextureMemory();
        }

        [DisplayName("Rendering Priority")]
        public D2D1_RENDERING_PRIORITY RenderingPriority { get; }

        [DisplayName("Maximum Texture Memory")]
        public ulong MaximumTextureMemory { get; }

        public override string ToString() => _text;
    }
}
