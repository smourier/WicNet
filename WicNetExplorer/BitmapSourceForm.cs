using System;
using System.Drawing;
using System.Windows.Forms;
using DirectN;
using WicNet;
using WicNet.Utilities;

namespace WicNetExplorer;

public partial class BitmapSourceForm : Form
{
    private IComObject<ID2D1Bitmap1>? _bitmap;
    private readonly D2DHwndRenderTargetControl _d2d = new();

    public BitmapSourceForm(WicBitmapSource bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        bitmap.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        BitmapSource = bitmap;
        InitializeComponent();
        Icon = Resources.WicNetIcon;

        _d2d.Draw += (s, e) =>
        {
            if (_bitmap == null)
            {
                using var dc = e.DeviceContext.AsComObject<ID2D1DeviceContext>();
                _bitmap = dc.CreateBitmapFromWicBitmap(BitmapSource.ComObject);
                e.DeviceContext.Clear(_d2d.BackColor.ToD3DCOLORVALUE());
            }

            if (_bitmap != null)
            {
                // keep proportions
                var size = _bitmap.GetSize();
                var factor = size.GetScaleFactor(_d2d.Width, _d2d.Height);
                var rc = new D2D_RECT_F(0, 0, size.width * factor.width, size.height * factor.height);
                e.DeviceContext.DrawBitmap(_bitmap, 1, interpolationMode: D2D1_BITMAP_INTERPOLATION_MODE.D2D1_BITMAP_INTERPOLATION_MODE_LINEAR, destinationRectangle: rc);
            }
        };
        _d2d.Dock = DockStyle.Fill;
        _d2d.BackColor = Color.AliceBlue;
        Controls.Add(_d2d);
    }

    public WicBitmapSource BitmapSource { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _bitmap.SafeDispose();
            _d2d?.Dispose();
        }
        base.Dispose(disposing);
    }
}
