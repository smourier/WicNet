using System;
using System.ComponentModel;
using System.Drawing.Design;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    public class PreviewBitmapSourceModel : BitmapSourceModel, IDisposable
    {
        public PreviewBitmapSourceModel(WicBitmapSource bitmap)
            : base(bitmap)
        {
            //Preview = "<Click here on the button to preview>";
            Preview = bitmap;
        }

        [Browsable(false)]
        [Editor(typeof(BitmapSourceModelEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(StringFormatterConverter))]
        [StringFormatter("ClickHereForPreview", ResourcesType = typeof(Resources))]
        public WicBitmapSource Preview { get; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Preview?.Dispose();
        }
    }
}
