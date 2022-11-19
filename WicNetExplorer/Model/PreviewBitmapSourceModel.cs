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
            Preview = bitmap;
        }

        protected override bool EnableThumbnail => false;

        [ToStringVisitor(Ignore = true)]
        [Editor(typeof(BitmapSourceModelEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("ClickHereForPreview", ResourcesType = typeof(Resources))]
        public WicBitmapSource Preview { get; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Preview?.Dispose();
        }
    }
}
