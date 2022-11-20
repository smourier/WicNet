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
        private WicColorContext? _colorContext;
        private IComObject<ID2D1Effect>? _colorManagementEffect;
        private readonly ID2Control _d2d;

        public ImageForm()
        {
            InitializeComponent();
            Icon = Resources.WicNetIcon;

            //if (!Program.ForceWindows7Mode && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134))
            //{
            //    _d2d = new D2DCompositionControl();
            //}
            //else
            {
                _d2d = new D2DControl();
            }

            var ctl = (Control)_d2d;
            _d2d.Draw += (s, e) =>
            {
                IComObject<ID2D1DeviceContext>? dc = null;
                IComObject<ID2D1ColorContext>? ctx = null;
                if (_bitmapSource != null)
                {
                    if (_bitmap == null)
                    {
                        dc = e.Target.AsComObject<ID2D1DeviceContext>(true);

                        if (_colorContext != null)
                        {
                            ctx = dc.CreateColorContextFromWicColorContext(_colorContext.ComObject);
                            _colorManagementEffect = dc.CreateEffect(Direct2DEffects.CLSID_D2D1ColorManagement);

                            //_colorManagementEffect.SetValue((int)D2D1_COLORMANAGEMENT_PROP.D2D1_COLORMANAGEMENT_PROP_QUALITY, D2D1_COLORMANAGEMENT_QUALITY.D2D1_COLORMANAGEMENT_QUALITY_BEST);
                            _colorManagementEffect.SetValue((int)D2D1_COLORMANAGEMENT_PROP.D2D1_COLORMANAGEMENT_PROP_SOURCE_COLOR_CONTEXT, ctx);
                        }

                        //var p = new D2D1_BITMAP_PROPERTIES1();
                        //p.pixelFormat.format = DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM;
                        _bitmap = dc.CreateBitmapFromWicBitmap(_bitmapSource.ComObject);
                        _bitmapSource.Dispose();
                        _bitmapSource = null;
                        _colorContext?.Dispose();
                        _colorContext = null;
                    }
                }

                if (_bitmap != null)
                {
                    e.Target.Clear(_D3DCOLORVALUE.FromColor(ctl.BackColor));

                    // keep proportions
                    var size = _bitmap.GetSize();
                    var factor = size.GetScaleFactor(ctl.Width, ctl.Height);
                    var rc = new D2D_RECT_F(0, 0, size.width * factor.width, size.height * factor.height);

                    if (_colorManagementEffect != null)
                    {
                        _colorManagementEffect.SetInput(0, _bitmap);
                        dc.DrawImage(_colorManagementEffect);
                    }
                    else
                    {
                        e.Target.DrawBitmap(_bitmap, interpolationMode: D2D1_BITMAP_INTERPOLATION_MODE.D2D1_BITMAP_INTERPOLATION_MODE_LINEAR, destinationRectangle: rc);
                    }
                    e.Handled = true;
                }
                dc?.Dispose();
                ctx?.Dispose();
            };

            ctl.Dock = DockStyle.Fill;
            ctl.BackColor = Color.AliceBlue;
            Controls.Add(ctl);
        }

        public string? FileName { get; private set; }

        private void ButtonMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        public static ImageForm? OpenFile(Form parent, string? fileName = null)
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
                    return null;

                fileName = fd.FileName;
            }
            if (!IOUtilities.PathIsFile(fileName))
                return null;

            var imageForm = new ImageForm
            {
                MdiParent = parent
            };
            if (!imageForm.LoadFile(fileName))
            {
                imageForm.Close();
                return null;
            }

            imageForm.Show();

            Settings.Current.AddRecentFile(fileName);
            Settings.Current.SerializeToConfiguration();
            return imageForm;
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
                DisposeResources();
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

        public bool LoadFile(string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);

            DisposeResources();
            try
            {
                _bitmapSource = WicBitmapSource.Load(fileName);
            }
            catch (Exception e)
            {
                this.ShowError(e.GetAllMessages());
                return false;
            }

            if (Settings.Current.HonorColorContexts)
            {
                _colorContext = _bitmapSource.GetBestColorContext();
            }

            var orientation = Settings.Current.HonorOrientation ? _bitmapSource.GetOrientation() : null;

            if (_bitmapSource.WicPixelFormat.NumericRepresentation == WICPixelFormatNumericRepresentation.WICPixelFormatNumericRepresentationFloat)
            {
                //_bitmapSource.ConvertTo(WicPixelFormat.GUID_WICPixelFormat64bppPRGBA);
            }
            else
            {
                _bitmapSource.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
            }

            // rotate after conversion
            if (orientation.HasValue)
            {
                // note: WIC is clockwise, while metadata is counter-clockwise...
                switch (orientation.Value)
                {
                    case PHOTO_ORIENTATION.FLIPHORIZONTAL:
                        _bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal);
                        break;

                    case PHOTO_ORIENTATION.ROTATE180:
                        _bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate180);
                        break;

                    case PHOTO_ORIENTATION.FLIPVERTICAL:
                        _bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformFlipVertical);
                        break;

                    case PHOTO_ORIENTATION.TRANSPOSE:
                        _bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate270 | WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal);
                        break;

                    case PHOTO_ORIENTATION.ROTATE270:
                        _bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate90);
                        break;

                    case PHOTO_ORIENTATION.TRANSVERSE:
                        _bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate270 | WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal);
                        break;

                    case PHOTO_ORIENTATION.ROTATE90:
                        _bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate270);
                        break;
                }
            }

            _d2d.Invalidate();

            Text = fileName;
            FileName = fileName;

            if (_bitmapSource.Width > 0 && _bitmapSource.Height > 0)
            {
                var bounds = MdiBounds;
                Size newSize;
                if (_bitmapSource.Width > _bitmapSource.Height && _bitmapSource.Width > 0)
                {
                    newSize = new(bounds.Width, (int)(bounds.Width * (_bitmapSource.Height / (float)_bitmapSource.Width)));
                }
                else
                {
                    newSize = new((int)(bounds.Height * _bitmapSource.Width / (float)_bitmapSource.Height), bounds.Height);
                }
                MdiResizeClient(newSize);
            }
            return true;
        }

        public void SaveFile() => SaveFile(false);
        public void SaveFileAs() => SaveFile(true);
        private void SaveFile(bool choose)
        {
            if (_bitmap == null || _d2d.DeviceContext == null)
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
            using (var device = _d2d.DeviceContext.GetDevice())
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
            DisposeResources();
            _d2d.Invalidate();
        }

        private void DisposeResources()
        {
            _bitmap?.Dispose();
            _bitmap = null;
            _colorManagementEffect?.Dispose();
            _colorManagementEffect = null;
            _bitmapSource?.Dispose();
            _bitmapSource = null;
            _colorContext?.Dispose();
            _colorContext = null;
        }
    }
}
