using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DirectN;
using WicNet;
using WicNet.Utilities;
using WicNetExplorer.Utilities;
using Windows.Data.Pdf;
using Windows.Storage;
using Extensions = WicNetExplorer.Utilities.Extensions;

namespace WicNetExplorer;

public partial class ImageForm : MdiForm
{
    private static readonly Lock _lock = new();
    private IComObject<ID2D1Bitmap1>? _bitmap;
    private IComObject<ID2D1BitmapBrush>? _backgroundBrush;
    private WicBitmapSource? _bitmapSource;
    private IComObject<IWICColorContext>? _colorContext;
    private IComObject<ID2D1Effect>? _colorManagementEffect;
    private IComObject<ID2D1Effect>? _scaleEffect;
    private IComObject<ID2D1SvgDocument>? _svgDocument;
    private ID2DControl? _d2d;
    private PdfDocument? _pdfDocument;
    private IPdfRendererNative? _pdfRendererNative;
    private Button? _nextPage;
    private Button? _previousPage;
    private Button? _play;
    private int _currentPage;
    private int _decoderFrameCount = 1; // for animated images
    private int _animationDelay;
    private bool _playingAnimation;

    public ImageForm()
    {
        InitializeComponent();
        Icon = Resources.WicNetIcon;
    }

    public ID2DControl? D2DControl => _d2d;
    public bool PlayingAnimation => _playingAnimation;
    public IComObject<ID2D1Bitmap1>? Bitmap => _bitmap;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? FileName { get; private set; }

    protected virtual void EnsureD2DControl()
    {
        if (_d2d != null)
            return;

        _d2d = CreateD2DControl();
        var ctl = (Control)_d2d;
        _d2d.Draw += (s, e) => DoDraw(e.DeviceContext);
        ctl.Dock = DockStyle.Fill;
        var color = Settings.Current.BackgroundColor;
        if (color != Color.Transparent)
        {
            ctl.BackColor = color;
        }
        Controls.Add(ctl);
    }

    public virtual void DoDraw(IComObject<ID2D1DeviceContext> deviceContext)
    {
        if (FileName != null)
        {
            if (Extensions.IsSvg(FileName))
            {
                DoDrawSvg(deviceContext);
                return;
            }

            if (Extensions.IsPdf(FileName))
            {
                DoDrawPdf(deviceContext);
                return;
            }
        }

        DoDrawBitmap(deviceContext);
    }

    protected virtual void DoDrawPdf(IComObject<ID2D1DeviceContext> deviceContext)
    {
        ArgumentNullException.ThrowIfNull(deviceContext);
        deviceContext.Clear(_D3DCOLORVALUE.White);
        if (!Extensions.IsPdfDocumentSupported())
            return;

        if (_pdfDocument == null || _pdfDocument.PageCount == 0)
            return;

        if (_pdfRendererNative == null)
        {
            using var device = deviceContext.GetDevice<ID2D1Device2>();
            device.Object.GetDxgiDevice(out var dxgi);
            if (dxgi == null) // HwndRenderTarget has no underlying dxgi device
                return;

            _pdfRendererNative = Extensions.PdfCreateRenderer(dxgi, false);
        }

        var page = _pdfDocument.GetPage((uint)_currentPage);
        var unk = Marshal.GetIUnknownForObject(page);
        try
        {
            // keep proportions
            var size = new D2D_SIZE_F(page.Size.Width, page.Size.Height);
            var factor = size.GetScaleFactor(Width, Height);
            using var mem = new ComMemory(new PDF_RENDER_PARAMS
            {
                BackgroundColor = _D3DCOLORVALUE.White,
                DestinationWidth = (int)(size.width * factor.width),
                DestinationHeight = (int)(size.height * factor.height),
                IgnoreHighContrast = Settings.Current.PdfIgnoreHighContrast,
            });
            _pdfRendererNative?.RenderPageToDeviceContext(unk, deviceContext.Object, mem.Pointer);
        }
        finally
        {
            Marshal.Release(unk);
        }
    }

    protected virtual void DoDrawSvg(IComObject<ID2D1DeviceContext> deviceContext)
    {
        ArgumentNullException.ThrowIfNull(deviceContext);
        if (_d2d == null)
            throw new InvalidProgramException();

        if (Settings.Current.UseBackgroundColorForSvgTransparency)
        {
            DrawBackground(deviceContext);
        }

        var dc5 = deviceContext.As<ID2D1DeviceContext5>();
        if (dc5 == null)
            return;

        if (_svgDocument == null)
        {
            using var stream = new WicNet.Utilities.UnmanagedMemoryStream(FileName);
            _svgDocument = dc5.CreateSvgDocument(stream, new D2D_SIZE_F(Width, Height));
        }

        if (_svgDocument != null)
        {
            dc5.DrawSvgDocument(_svgDocument.Object);
        }
    }

    protected virtual void DoDrawBitmap(IComObject<ID2D1DeviceContext> deviceContext)
    {
        ArgumentNullException.ThrowIfNull(deviceContext);
        if (_d2d == null)
            throw new InvalidProgramException();

        lock (_lock)
        {
            var ctl = (Control)_d2d;
            IComObject<ID2D1ColorContext>? ctx = null;
            if (_bitmapSource != null && !_bitmapSource.ComObject.IsDisposed)
            {
                if (_bitmap == null || _bitmap.IsDisposed)
                {
                    var pf = deviceContext.GetPixelFormat().format;
                    if ((_colorContext != null && !_colorContext.IsDisposed) || pf == DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT)
                    {
                        _colorManagementEffect = deviceContext.CreateEffect(Direct2DEffects.CLSID_D2D1ColorManagement);
                        _scaleEffect = deviceContext.CreateEffect(Direct2DEffects.CLSID_D2D1Scale);
                        _scaleEffect.SetValue((int)D2D1_SCALE_PROP.D2D1_SCALE_PROP_INTERPOLATION_MODE, D2D1_SCALE_INTERPOLATION_MODE.D2D1_SCALE_INTERPOLATION_MODE_HIGH_QUALITY_CUBIC);

                        if (pf == DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT)
                        {
                            using var scRGB = deviceContext.As<ID2D1DeviceContext5>().CreateColorContextFromDxgiColorSpace(DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RGB_FULL_G10_NONE_P709);
                            _colorManagementEffect.SetValue((int)D2D1_COLORMANAGEMENT_PROP.D2D1_COLORMANAGEMENT_PROP_DESTINATION_COLOR_CONTEXT, scRGB);
                        }

                        if (_colorContext == null || _colorContext.IsDisposed)
                        {
                            var isFloat = _bitmapSource.WicPixelFormat.NumericRepresentation == WICPixelFormatNumericRepresentation.WICPixelFormatNumericRepresentationFloat;
                            ctx = deviceContext.CreateColorContext(isFloat ? D2D1_COLOR_SPACE.D2D1_COLOR_SPACE_SCRGB : D2D1_COLOR_SPACE.D2D1_COLOR_SPACE_SRGB);
                        }
                        else
                        {
                            ctx = deviceContext.CreateColorContextFromWicColorContext(_colorContext);
                        }

                        _colorManagementEffect.SetValue((int)D2D1_COLORMANAGEMENT_PROP.D2D1_COLORMANAGEMENT_PROP_QUALITY, D2D1_COLORMANAGEMENT_QUALITY.D2D1_COLORMANAGEMENT_QUALITY_BEST);
                        _colorManagementEffect.SetValue((int)D2D1_COLORMANAGEMENT_PROP.D2D1_COLORMANAGEMENT_PROP_SOURCE_COLOR_CONTEXT, ctx);
                    }

                    // check max Direct2D bitmap size
                    var maxSize = deviceContext.Object.GetMaximumBitmapSize();
                    if (_bitmapSource.Width > maxSize || _bitmapSource.Height > maxSize)
                    {
                        var scalingMode = (WICBitmapInterpolationMode)Settings.Current.ScalingInterpolationMode;
                        var format = _bitmapSource.PixelFormat;
                        _bitmapSource.Scale((int)maxSize, scalingMode);
                        if (_bitmapSource.PixelFormat != format)
                        {
                            _bitmapSource.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
                        }
                    }

                    _bitmap = deviceContext.CreateBitmapFromWicBitmap(_bitmapSource.ComObject);
                    _colorContext.SafeDispose();
                    _colorContext = null;
                }
            }

            if (_bitmap != null && !_bitmap.IsDisposed)
            {
                DrawBackground(deviceContext);

                // keep proportions
                var size = _bitmap.GetSize();
                var factor = size.GetScaleFactor(ctl.Width, ctl.Height);
                var rc = new D2D_RECT_F(0, 0, size.width * factor.width, size.height * factor.height);

                if (_colorManagementEffect != null && !_colorManagementEffect.IsDisposed)
                {
                    _scaleEffect.SetValue((int)D2D1_SCALE_PROP.D2D1_SCALE_PROP_SCALE, factor.ToD2D_VECTOR_2F());
                    _scaleEffect.SetInput(_bitmap);
                    _colorManagementEffect.SetInput(_scaleEffect);
                    deviceContext.DrawImage(_colorManagementEffect);
                }
                else
                {
                    deviceContext.DrawBitmap(_bitmap, 1, D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_HIGH_QUALITY_CUBIC, rc);
                }
            }
            ctx.SafeDispose();
        }
    }

    protected virtual void DrawBackground(IComObject<ID2D1DeviceContext> deviceContext)
    {
        ArgumentNullException.ThrowIfNull(deviceContext);
        if (_d2d == null)
            throw new InvalidProgramException();

        var color = Settings.Current.BackgroundColor;
        if (color.A == 0 && color.R == 0 && color.G == 0 && color.B == 0 || color == Color.Transparent)
        {
            _backgroundBrush ??= deviceContext.CreateCheckerboardBrush(8);
            deviceContext.Clear(_D3DCOLORVALUE.White);
            deviceContext.FillRectangle(new D2D_RECT_F(0, 0, Width, Height), _backgroundBrush);
        }
        else
        {
            deviceContext.Clear(color.ToD3DCOLORVALUE());
        }
    }

    private void ButtonMinimize_Click(object sender, EventArgs e)
    {
        WindowState = FormWindowState.Minimized;
    }

    private ID2DControl CreateD2DControl()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134) || Settings.Current.ForceW7)
            return new D2DHwndRenderTargetControl();

        var ctl = new D2DCompositionControl();

        if (Settings.Current.ForceFP16 || _bitmapSource?.WicPixelFormat.NumericRepresentation == WICPixelFormatNumericRepresentation.WICPixelFormatNumericRepresentationFloat)
        {
            ctl.PixelFormat = DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT;
        }
        return ctl;
    }

    public static ImageForm? OpenFile(Form parent, string? fileName = null)
    {
        ArgumentNullException.ThrowIfNull(parent);
        if (fileName == null)
        {
            var exts = new List<string>(WicImagingComponent.DecoderFileExtensions)
            {
                ".svg",
                ".svgz"
            };

            if (Extensions.IsPdfDocumentSupported())
            {
                exts.Add(".pdf");
            }

            var filter = BuildFilters(exts);
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
            if (fd.ShowDialog(parent) != System.Windows.Forms.DialogResult.OK)
                return null;

            fileName = fd.FileName;
        }
        if (!IOUtilities.PathIsFile(fileName))
            return null;

        var imageForm = new ImageForm { MdiParent = parent };
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

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (_svgDocument != null)
        {
            _svgDocument.Object.SetViewportSize(new D2D_SIZE_F(Width, Height));
            _d2d?.WithDeviceContext(DoDraw);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeContextDependentResources();
            DisposeContextIndependentResources();
            _nextPage?.Dispose();
            _previousPage?.Dispose();
            _play?.Dispose();
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

    protected override void SetCaptionButtons()
    {
        base.SetCaptionButtons();
        var buttonHeight = Padding.Top - Padding.Bottom;
        if (_previousPage != null)
        {
            _previousPage.Height = buttonHeight;
            _previousPage.Location = new Point(Padding.Left * 2, Padding.Bottom + buttonHeight + 5);
            _previousPage.BringToFront();
        }

        if (_nextPage != null)
        {
            _nextPage.Height = buttonHeight;
            _nextPage.Location = new Point(Padding.Left * 3 + _previousPage!.Width, Padding.Bottom + buttonHeight + 5);
            _nextPage.BringToFront();
        }

        if (_play != null)
        {
            _play.Height = buttonHeight;
            _play.Location = new Point(Padding.Left * 4 + _previousPage!.Width + _nextPage!.Width, Padding.Bottom + buttonHeight + 5);
            _play.BringToFront();
        }
    }

    public virtual bool LoadFile(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        if (Extensions.IsSvg(fileName))
            return LoadSvgFile(fileName);

        if (Extensions.IsPdfDocumentSupported() && Extensions.IsPdf(fileName))
            return LoadPdfFile(fileName);

        return LoadBitmapFile(fileName, 0);
    }

    protected virtual bool LoadSvgFile(string fileName)
    {
        EnsureD2DControl();
        Text = fileName;
        FileName = fileName;
        return true;
    }

    [SupportedOSPlatform("windows10.0.10240.0")]
    protected virtual bool LoadPdfFile(string fileName)
    {
        EnsureD2DControl();
        Text = fileName;
        FileName = fileName;
        _ = LoadPdfDocument(FileName);
        return true;
    }

    [SupportedOSPlatform("windows10.0.10240.0")]
    protected async Task LoadPdfDocument(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        var file = await StorageFile.GetFileFromPathAsync(fileName);
        _pdfDocument = await PdfDocument.LoadFromFileAsync(file);

        Text = fileName + " - " + string.Format(Resources.Pages, _pdfDocument.PageCount);
        if (_pdfDocument.PageCount > 1 && Settings.Current.EnablePaging)
        {
            AddPageButtons(UpdatePdfPage);
        }

        _d2d?.Redraw();

        // some race condition prevents new text with pages count to be displayed
        Invalidate();
    }

    protected virtual void AddPageButtons(Action<int> updatePage, Action? playImage = null)
    {
        if (updatePage != null)
        {
            _nextPage = new Button
            {
                Text = Resources.NextPage,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
            };

            _nextPage.Click += (s, e) => updatePage(1);
            Controls.Add(_nextPage);

            _previousPage = new Button
            {
                Text = Resources.PreviousPage,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
            };

            _previousPage.Click += (s, e) => updatePage(-1);
            Controls.Add(_previousPage);
        }

        if (playImage != null)
        {
            _play = new Button
            {
                Text = Resources.Play,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
            };

            _play.Click += (s, e) => playImage();
            Controls.Add(_play);
        }
        SetCaptionButtons();
    }

    protected virtual void UpdatePdfPage(int delta)
    {
        if (delta == 0 || _pdfDocument == null || !Extensions.IsPdfDocumentSupported())
            return;

        var tentative = delta + _currentPage;
        if (tentative < 0)
        {
            tentative = (int)(_pdfDocument.PageCount - 1);
        }
        else if (tentative >= _pdfDocument.PageCount)
        {
            tentative = 0;
        }

        _currentPage = tentative;
        _nextPage!.Enabled = _currentPage < _pdfDocument.PageCount - 1;
        _previousPage!.Enabled = _currentPage > 0;
        _d2d?.Redraw();

        Text = FileName + " - " + string.Format(Resources.Pages, _pdfDocument.PageCount) + " - " + string.Format(Resources.Current, _currentPage);
        Invalidate();
    }

    protected virtual WicBitmapSource LoadBitmapFrame(WicBitmapDecoder decoder, int frameIndex)
    {
        ArgumentNullException.ThrowIfNull(decoder);

        var bitmapSource = decoder.GetFrame(frameIndex);

        // metadata must be read before it's converted or rotated
        var orientation = Settings.Current.HonorOrientation ? bitmapSource.GetOrientation() : null;

        // read GIF animation delay, if any
        using var reader = bitmapSource.GetMetadataReader();
        if (reader != null && reader.TryGetMetadataByName<int>("/grctlext/Delay", out var delay, out _))
        {
            _animationDelay = delay * 10;
        }
        else
        {
            // note WIC WEBP decoder doesn't support decoding this from metadata, so we use a default value
            _animationDelay = Settings.Current.DefaultAnimationDelay;
        }

        if (bitmapSource.WicPixelFormat.NumericRepresentation == WICPixelFormatNumericRepresentation.WICPixelFormatNumericRepresentationFloat)
        {
            bitmapSource.ConvertTo(WicPixelFormat.GUID_WICPixelFormat128bppRGBAFloat);
            bitmapSource.ConvertTo(WicPixelFormat.GUID_WICPixelFormat64bppPRGBAHalf);
        }
        else
        {
            bitmapSource.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        }

        // rotate after conversion
        if (orientation.HasValue)
        {
            // note: WIC is clockwise, while metadata is counter-clockwise...
            switch (orientation.Value)
            {
                case PHOTO_ORIENTATION.FLIPHORIZONTAL:
                    bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal);
                    break;

                case PHOTO_ORIENTATION.ROTATE180:
                    bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate180);
                    break;

                case PHOTO_ORIENTATION.FLIPVERTICAL:
                    bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformFlipVertical);
                    break;

                case PHOTO_ORIENTATION.TRANSPOSE:
                    bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate270 | WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal);
                    break;

                case PHOTO_ORIENTATION.ROTATE270:
                    bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate90);
                    break;

                case PHOTO_ORIENTATION.TRANSVERSE:
                    bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate270 | WICBitmapTransformOptions.WICBitmapTransformFlipVertical);
                    break;

                case PHOTO_ORIENTATION.ROTATE90:
                    bitmapSource.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate270);
                    break;
            }
        }

        return bitmapSource;
    }

    protected virtual bool LoadBitmapFile(string fileName, int frameIndex)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        DisposeContextDependentResources();
        DisposeContextIndependentResources();
        try
        {
            using var decoder = WicBitmapDecoder.Load(fileName);
            {
                _bitmapSource = LoadBitmapFrame(decoder, frameIndex);
            }
        }
        catch (Exception e)
        {
            this.ShowError(e.GetAllMessages());
            return false;
        }

        if (Settings.Current.HonorColorContexts)
        {
            var best = _bitmapSource.GetBestColorContext();
            _colorContext = best?.ComObject;
        }

        EnsureD2DControl();

        _decoderFrameCount = _bitmapSource.DecoderFrameCount;
        if (_decoderFrameCount > 1 && Settings.Current.EnablePaging)
        {
            Text = fileName + " - " + string.Format(Resources.Frames, _decoderFrameCount);
        }
        else
        {
            Text = fileName;
        }

        Invalidate();
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

        if (_decoderFrameCount > 1 && Settings.Current.EnablePaging)
        {
            AddPageButtons(UpdateBitmapPage, ToggleBitmapPlay);
        }
        return true;
    }

    protected virtual void ToggleBitmapPlay()
    {
        if (_decoderFrameCount <= 1 || FileName == null || _d2d == null)
            return;

        if (_playingAnimation)
        {
            _playingAnimation = false;
            _play!.Text = Resources.Play;
            _nextPage!.Enabled = true;
            _previousPage!.Enabled = true;
            return;
        }

        _play!.Text = Resources.Stop;
        _nextPage!.Enabled = false;
        _previousPage!.Enabled = false;

        _ = Task.Run(() =>
        {
            _playingAnimation = true;
            var honorColorContexts = Settings.Current.HonorColorContexts;
            var decoder = WicBitmapDecoder.Load(FileName);
            var frames = new (WicBitmapSource bmp, IComObject<IWICColorContext>? color)[decoder.FrameCount];
            for (var i = 0; i < decoder.FrameCount; i++)
            {
                frames[i] = new() { bmp = LoadBitmapFrame(decoder, i) };

                if (honorColorContexts)
                {
                    var best = frames[i].bmp.GetBestColorContext();
                    frames[i].color = best?.ComObject;
                }
            }

            while (_playingAnimation)
            {
                for (var i = 0; i < decoder.FrameCount; i++)
                {
                    _bitmapSource = frames[i].bmp;
                    _bitmap?.Dispose();
                    _bitmap = null;

                    _colorContext?.Dispose();
                    _colorContext = frames[i].color;

                    _colorManagementEffect?.Dispose();
                    _colorManagementEffect = null;

                    _scaleEffect?.Dispose();
                    _scaleEffect = null;

                    BeginInvoke(() =>
                    {
                        _d2d?.Redraw();
                        Text = FileName + " - " + string.Format(Resources.Frames, _decoderFrameCount) + " - " + string.Format(Resources.Current, i);
                        Invalidate();
                    });
                    Thread.Sleep(_animationDelay);
                }
            }

            foreach (var (bmp, color) in frames)
            {
                bmp?.Dispose();
                color?.Dispose();
            }
        });
    }

    protected virtual void UpdateBitmapPage(int delta)
    {
        if (delta == 0 || _decoderFrameCount <= 1 || FileName == null)
            return;

        var tentative = delta + _currentPage;
        if (tentative < 0)
        {
            tentative = _decoderFrameCount - 1;
        }
        else if (tentative >= _decoderFrameCount)
        {
            tentative = 0;
        }

        _currentPage = tentative;
        _nextPage!.Enabled = _currentPage < _decoderFrameCount - 1;
        _previousPage!.Enabled = _currentPage > 0;

        LoadBitmapFile(FileName, _currentPage);
        _d2d?.Redraw();
        Text = FileName + " - " + string.Format(Resources.Frames, _decoderFrameCount) + " - " + string.Format(Resources.Current, _currentPage);
        Invalidate();
    }

    public void SaveFile() => SaveFile(false);
    public void SaveFileAs() => SaveFile(true);
    private void SaveFile(bool choose)
    {
        if (_d2d == null)
            return;

        var dispose = false;
        var bitmap = _bitmap;
        if (bitmap == null)
        {
            bitmap = _d2d.GetSurfaceBitmap();
            if (bitmap == null)
            {
                this.ShowError(Resources.SaveNotSupported);
                return;
            }

            dispose = true;
        }

        try
        {
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
                if (fd.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                    return;

                fileName = fd.FileName;
            }

            var encoder = WicEncoder.FromFileExtension(Path.GetExtension(fileName)) ?? throw new WicNetException("WIC0003: Cannot determine encoder from file path.");
            IOUtilities.FileDelete(fileName, true, false);
            _d2d.WithDeviceContext(dc =>
            {
                using var device = dc.GetDevice();
                using var image = bitmap.AsComObject<ID2D1Image>(true);
                using var stream = File.OpenWrite(fileName);
                image.Save(device, encoder.ContainerFormat, stream);
                DoDraw(dc);
            });

            if (new FileInfo(fileName).Length == 0)
            {
                IOUtilities.FileDelete(fileName, true, false);
            }

            LoadFile(fileName);
        }
        finally
        {
            if (dispose)
            {
                bitmap.Dispose();
            }
        }
    }

    private void CloseFile()
    {
        FileName = null;
        DisposeContextIndependentResources();
        DisposeContextDependentResources();
        (_d2d as Control)?.Invalidate();
    }

    private void DisposeContextIndependentResources()
    {
        _bitmapSource?.Dispose();
        _bitmapSource = null;
        _colorContext.SafeDispose();
        _colorContext = null;
    }

    private void DisposeContextDependentResources()
    {
        _backgroundBrush.SafeDispose();
        _backgroundBrush = null;
        _bitmap.SafeDispose();
        _bitmap = null;
        _colorManagementEffect.SafeDispose();
        _colorManagementEffect = null;
        _scaleEffect.SafeDispose();
        _scaleEffect = null;
        _svgDocument.SafeDispose();
        _svgDocument = null;
        _bitmapSource?.Dispose();
        _bitmapSource = null;
        _colorContext.SafeDispose();
        _colorContext = null;
    }
}
