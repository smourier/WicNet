using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DirectN;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer
{
    public partial class ImageForm : MdiForm
    {
        private IComObject<ID2D1Bitmap1>? _bitmap;
        private WicBitmapSource? _bitmapSource;
        private readonly D2DControl _d2d = new();

        public ImageForm()
        {
            InitializeComponent();
            //BackColor = Color.Red;
            Icon = Resources.WicNetIcon;

            _d2d.Draw += (s, e) =>
            {
                if (_bitmapSource != null)
                {
                    if (_bitmap == null)
                    {
                        using var dc = e.Target.AsComObject<ID2D1DeviceContext>();
                        _bitmap = dc.CreateBitmapFromWicBitmap(_bitmapSource.ComObject);
                        _bitmapSource.Dispose();
                        _bitmapSource = null;
                    }
                }

                if (_bitmap != null)
                {
                    e.Target.Clear(_D3DCOLORVALUE.FromColor(_d2d.BackColor));

                    // keep proportions
                    var size = _bitmap.GetSize();
                    var factor = size.GetScaleFactor(_d2d.Width, _d2d.Height);
                    var rc = new D2D_RECT_F(0, 0, size.width * factor.width, size.height * factor.height);
                    e.Target.DrawBitmap(_bitmap, interpolationMode: D2D1_BITMAP_INTERPOLATION_MODE.D2D1_BITMAP_INTERPOLATION_MODE_LINEAR, destinationRectangle: rc);
                    e.Handled = true;
                }
            };
            _d2d.Dock = DockStyle.Fill;
            _d2d.BackColor = Color.AliceBlue;
            Controls.Add(_d2d);
        }

        public string? FileName { get; private set; }

        private void ButtonMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        public static void OpenFile(Form parent, string? fileName = null)
        {
            ArgumentNullException.ThrowIfNull(parent);
            if (fileName == null)
            {
                var filter = BuildFilters(WicImagingComponent.DecoderFileExtensions);
                var fd = new OpenFileDialog
                {
                    RestoreDirectory = true,
                    Multiselect = false,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = filter.Item1,
                    // note the "all image" list is super long but what other choice do we have?
                    FilterIndex = filter.Item2, // select all images by default
                };
                if (fd.ShowDialog(parent) != DialogResult.OK)
                    return;

                fileName = fd.FileName;
            }
            if (!IOUtilities.PathIsFile(fileName))
                return;

            var imageForm = new ImageForm();
            imageForm.MdiParent = parent;
            imageForm.LoadFile(fileName);
            imageForm.Show();

            Settings.Current.AddRecentFile(fileName);
            Settings.Current.SerializeToConfiguration();
        }

        private static Tuple<string, int> BuildFilters(IEnumerable<string> extensions)
        {
            var allExts = extensions.OrderBy(ext => ext).ToArray();
            var imageExts = string.Join(";", allExts.Select(ext => "*" + ext));
            return new Tuple<string, int>(string.Join("|", allExts.Select(ext => string.Format(Resources.OneImageFileFilter, ext[1..].ToUpperInvariant(), ext))) + string.Format(Resources.AllImagesFilesFilter, imageExts) + Resources.AllFilesFilter, allExts.Length + 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bitmap?.Dispose();
                _bitmapSource?.Dispose();
                components?.Dispose();
                _d2d?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            CloseFile();
            base.OnClosing(e);
        }

        public void LoadFile(string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);

            _bitmap?.Dispose();
            _bitmap = null;
            _bitmapSource?.Dispose();

            _bitmapSource = WicBitmapSource.Load(fileName);
            _bitmapSource.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
            _d2d.Invalidate();

            Text = fileName;
            FileName = fileName;
        }

        public void SaveFile() => SaveFile(false);
        public void SaveFileAs() => SaveFile(true);
        private void SaveFile(bool choose)
        {
            if (_bitmap == null || _d2d.Target == null)
                return;

            var fileName = FileName;
            if (fileName == null || choose)
            {
                var filter = BuildFilters(WicImagingComponent.EncoderFileExtensions);
                var fd = new SaveFileDialog
                {
                    RestoreDirectory = true,
                    CheckPathExists = true,
                    Filter = filter.Item1,
                    FilterIndex = filter.Item2, // select all images by default
                };
                if (fd.ShowDialog(this) != DialogResult.OK)
                    return;

                fileName = fd.FileName;
            }

            var encoder = WicEncoder.FromFileExtension(Path.GetExtension(fileName));
            if (encoder == null)
                throw new WicNetException("WIC0003: Cannot determine encoder from file path.");

            IOUtilities.FileDelete(fileName, true, false);
            using (var device = _d2d.Target.As<ID2D1DeviceContext>(true).GetDevice())
            {
                using var image = _bitmap.AsComObject<ID2D1Image>(true);
                using var stream = File.OpenWrite(fileName);
                image.Save(device, encoder.ContainerFormat, stream);
            }

            if (new FileInfo(fileName).Length == 0)
            {
                IOUtilities.FileDelete(fileName, true, false);
            }

            LoadFile(fileName);
        }

        private void CloseFile()
        {
            FileName = null;
            _bitmap?.Dispose();
            _bitmap = null;
            _bitmapSource?.Dispose();
            _bitmapSource = null;
            _d2d.Invalidate();
        }
    }
}
