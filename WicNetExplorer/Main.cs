using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DirectN;
using WicNet;

namespace WicNetExplorer
{
    public partial class Main : Form
    {
        private IComObject<ID2D1Bitmap>? _bitmap;
        private WicBitmapSource? _bitmapSource;
        private readonly D2DControl _d2d = new();

        public Main()
        {
            InitializeComponent();
            Icon = Resources.WicNetIcon;

            _d2d.Draw += (s, e) =>
            {
                if (_bitmapSource != null)
                {
                    if (_bitmap == null)
                    {
                        _bitmap = e.Target.AsComObject<ID2D1DeviceContext>().CreateBitmapFromWicBitmap(_bitmapSource.ComObject);
                        _bitmapSource.Dispose();
                        _bitmapSource = null;
                    }
                }

                if (_bitmap != null)
                {
                    // keep proportions
                    var size = _bitmap.GetSize();
                    var factor = size.GetScaleFactor(_d2d.Width, _d2d.Height);
                    var rc = new D2D_RECT_F(0, 0, size.width * factor.width, size.height * factor.height); 
                    e.Target.DrawBitmap(_bitmap, interpolationMode: D2D1_BITMAP_INTERPOLATION_MODE.D2D1_BITMAP_INTERPOLATION_MODE_LINEAR, destinationRectangle: rc);
                }
                e.Handled = true;
            };
            _d2d.Dock = DockStyle.Fill;
            panelMain.Controls.Add(_d2d);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e) => Close();
        private void ToolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            var allExts = WicImagingComponent.DecoderFileExtensions.OrderBy(ext => ext).ToArray();
            var imageExts = string.Join(";", allExts.Select(ext => "*" + ext));
            var filter = string.Join("|", allExts.Select(ext => string.Format(Resources.OneImageFileFilter, ext[1..].ToUpperInvariant(), ext))) + string.Format(Resources.AllImagesFilesFilter, imageExts) + Resources.AllFilesFilter;
            var ofd = new OpenFileDialog
            {
                RestoreDirectory = true,
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = filter,
                // note the "all image" list is super long but what other choice do we have?
                FilterIndex = allExts.Length + 1 // select all images by default
            };
            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            _bitmap?.Dispose();
            _bitmap = null;
            _bitmapSource?.Dispose();
            _bitmapSource = WicBitmapSource.Load(ofd.FileName);
            _bitmapSource.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
            _d2d.Invalidate();

            // reset title
            new ComponentResourceManager(GetType()).ApplyResources(this, "$this");
            Text += " - " + ofd.FileName;
        }
    }
}