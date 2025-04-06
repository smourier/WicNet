using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DirectN;
using WicNet.Utilities;

namespace WicNetExplorer.Utilities
{
    // special form for MDI support that avoids ugly XP/Vista/8 style blue border
    // and supports Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
    public class MdiForm : Form
    {
        private Font? _buttonFont;
        private Font? _textFont;
        private float? _initialFontSize;
        private readonly Button _closeButton = new() { Text = "r", FlatStyle = FlatStyle.Flat, Name = Resources.Close };
        private readonly Button _restoreButton = new() { Text = "1", FlatStyle = FlatStyle.Flat, Name = Resources.Restore };
        private readonly Button _minimizeButton = new() { Text = "0", FlatStyle = FlatStyle.Flat, Name = Resources.Minimize };

        private const int _frameSize = 4;

        public MdiForm()
        {
            SuspendLayout();
            FormBorderStyle = FormBorderStyle.None; // we draw most of it ourselves
            AutoScaleMode = AutoScaleMode.Font; // dpi support
            Name = "MdiForm";
            Text = Name;

            _minimizeButton.FlatAppearance.BorderSize = 0;
            _minimizeButton.FlatAppearance.BorderColor = BackColor;
            _minimizeButton.Click += (s, e) => WindowState = FormWindowState.Minimized;
            Controls.Add(_minimizeButton);

            _restoreButton.FlatAppearance.BorderSize = 0;
            _restoreButton.FlatAppearance.BorderColor = BackColor;
            _restoreButton.Click += (s, e) => Restore();
            Controls.Add(_restoreButton);

            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.FlatAppearance.BorderColor = BackColor;
            _closeButton.Click += (s, e) => Close();
            Controls.Add(_closeButton);

            DoubleBuffered = true;
            ResumeLayout(false);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsActive { get; private set; }
        public Rectangle MdiBounds
        {
            get
            {
                var padding = Padding;
                return new(padding.Left, padding.Top, Width - padding.Horizontal, Height - padding.Vertical);
            }
        }

        public virtual void MdiResizeClient(Size size)
        {
            var padding = Padding;
            ClientSize = new Size(padding.Horizontal + size.Width, padding.Vertical + size.Height);
        }

        // for some reason, the builtin LayoutMdi command doesn't work with FormBorderStyle = None
        public static void LayoutMdi(Form mdiParent, MdiLayout layout)
        {
            ArgumentNullException.ThrowIfNull(mdiParent);
            var children = mdiParent.MdiChildren;
            if (children.Length == 0)
                return;

            const int pad = _frameSize;

            Size size;
            int width;
            int height;
            switch (layout)
            {
                case MdiLayout.TileHorizontal:
                    size = mdiParent.ClientSize;
                    width = size.Width - pad * 3;

                    height = size.Height;
                    if (mdiParent.MainMenuStrip != null)
                    {
                        height -= mdiParent.MainMenuStrip.Height + mdiParent.MainMenuStrip.Padding.Vertical;
                    }
                    height = (height - pad * (children.Length + 1)) / children.Length;

                    for (var i = 0; i < children.Length; i++)
                    {
                        var child = children[i];
                        child.WindowState = FormWindowState.Normal;

                        child.Left = pad;
                        child.Top = pad + i * (height + pad);
                        child.Width = width;
                        child.Height = height;
                    }
                    break;

                case MdiLayout.TileVertical:
                    size = mdiParent.ClientSize;
                    width = (size.Width - pad * (children.Length + 2)) / children.Length;
                    height = size.Height - pad * 2;
                    if (mdiParent.MainMenuStrip != null)
                    {
                        height -= mdiParent.MainMenuStrip.Height + mdiParent.MainMenuStrip.Padding.Vertical;
                    }

                    for (var i = 0; i < children.Length; i++)
                    {
                        var child = children[i];
                        child.WindowState = FormWindowState.Normal;

                        child.Left = pad + i * (width + pad);
                        child.Top = pad;
                        child.Width = width;
                        child.Height = height;
                    }
                    break;

                case MdiLayout.Cascade:
                    // for some reason, the CascadeWindows api doesn't seem to work either
                    var captionSize = WindowsUtilities.GetWindowCaptionRect(mdiParent.Handle);
                    var offset = Math.Abs(captionSize.top);
                    var current = _frameSize;
                    foreach (var child in children)
                    {
                        child.WindowState = FormWindowState.Normal;
                        child.Left = current;
                        child.Top = current;
                        child.BringToFront();

                        current += offset;
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _closeButton?.Dispose();
                _restoreButton?.Dispose();
                _minimizeButton?.Dispose();
                _buttonFont?.Dispose();
                _textFont?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == MessageDecoder.WM_SYSCOMMAND)
            {
                const int SC_RESTORE2 = 0xF122;
                if (m.WParam == new IntPtr(SC_RESTORE2))
                {
                    m.Result = IntPtr.Zero;
                    WindowState = FormWindowState.Normal;
                    return;
                }

                // this is needed to avoid infinite recursion (obscure winforms bug?)
                if (WindowState == FormWindowState.Minimized)
                {
                    m.Result = IntPtr.Zero;
                    return;
                }
            }

            if (m.Msg == MessageDecoder.WM_MOUSEACTIVATE)
            {
                base.WndProc(ref m);
                // not sure why me must do this ourselves
                if (m.Result == (IntPtr)1 || m.Result == (IntPtr)2)
                {
                    Activate();
                }
            }

            // emulate frame sizing & caption move
            if (m.Msg == MessageDecoder.WM_NCHITTEST)
            {
                var screenLeftTop = Parent!.PointToScreen(new Point(Left, Top));
                var screenX = m.LParam.GET_X_LPARAM();
                var screenY = m.LParam.GET_Y_LPARAM();
                var clientX = screenX - screenLeftTop.X;
                var clientY = screenY - screenLeftTop.Y;

                var frameSize = Padding.Left;

                if (clientX >= 0 && clientX < Width && clientY >= 0 && clientY <= Height)
                {
                    if (clientX < frameSize)
                    {
                        if (clientY <= frameSize)
                        {
                            m.Result = (IntPtr)HT.HTTOPLEFT;
                            return;
                        }

                        if (clientY >= (Height - frameSize))
                        {
                            m.Result = (IntPtr)HT.HTBOTTOMLEFT;
                            return;
                        }

                        m.Result = (IntPtr)HT.HTLEFT;
                        return;
                    }

                    if (clientX > (Width - frameSize))
                    {
                        if (clientY <= frameSize)
                        {
                            m.Result = (IntPtr)HT.HTTOPRIGHT;
                            return;
                        }

                        if (clientY >= (Height - frameSize))
                        {
                            m.Result = (IntPtr)HT.HTBOTTOMRIGHT;
                            return;
                        }

                        m.Result = (IntPtr)HT.HTRIGHT;
                        return;
                    }

                    if (clientY < frameSize)
                    {
                        m.Result = (IntPtr)HT.HTTOP;
                        return;
                    }

                    if (clientY > (Height - frameSize))
                    {
                        m.Result = (IntPtr)HT.HTBOTTOM;
                        return;
                    }

                    var padding = GetCaptionPadding();
                    var captionRc = new tagRECT(padding.Left, padding.Bottom, Width - padding.Right, padding.Top);
                    if (captionRc.Contains(clientX, clientY))
                    {
                        m.Result = (IntPtr)HT.HTCAPTION;
                        return;
                    }

                    m.Result = (IntPtr)HT.HTCLIENT;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (WindowState == FormWindowState.Normal)
            {
                RecomputeSizes();
                var height = Padding.Top;

                var brush = IsActive ? SystemBrushes.ActiveCaption : SystemBrushes.InactiveCaption;
                e.Graphics.FillRectangle(brush, Padding.Left, Padding.Bottom, Width - Padding.Horizontal, height);

                var size = TextRenderer.MeasureText(Text, _textFont);

                var flags = TextFormatFlags.PreserveGraphicsClipping; // make sure Grapchis.SetClip below works
                var left = Padding.Left;
                var maxWidth = _minimizeButton.Left - _frameSize * 2;
                if (size.Width > maxWidth)
                {
                    // case of smaller sizes
                    left -= size.Width - maxWidth;
                    e.Graphics.SetClip(new Rectangle(_frameSize * 2, _frameSize, maxWidth, height));
                }

                TextRenderer.DrawText(e.Graphics, Text, _textFont, new Rectangle(left, (height - size.Height) / 2, size.Width, size.Height), IsActive ? SystemColors.ActiveCaptionText : SystemColors.GrayText, flags);
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            SetCaptionColors();
            InvalidateCaption();
            IsActive = true;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            SetCaptionColors();
            InvalidateCaption();
            IsActive = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecomputeSizes();
            InvalidateCaption();
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            if (!IsActive)
            {
                Activate();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RecomputeSizes();
            this.EnsureStartupDpi();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            InvalidateCaption();
        }

        private void InvalidateCaption()
        {
            var padding = Padding;
            var caption = new Rectangle(0, 0, Width, padding.Top);
            Invalidate(caption);
        }

        protected virtual void RecomputeSizes()
        {
            SetFonts();
            Padding = GetCaptionPadding();
            SetCaptionButtons();
        }

        protected virtual void SetFonts()
        {
            _initialFontSize ??= Font.Size;
            var fontSize = DpiUtilities.AdjustForWindowDpi(_initialFontSize.Value, Handle);

            _textFont?.Dispose();
            _textFont = new Font(Font.FontFamily.Name, fontSize);

            _buttonFont?.Dispose();
            _buttonFont = new Font("Marlett", fontSize);
        }

        protected virtual void SetCaptionColors()
        {
            BackColor = IsActive ? SystemColors.InactiveCaption : SystemColors.ActiveCaption;
            _closeButton.BackColor = BackColor;
            _restoreButton.BackColor = BackColor;
            _minimizeButton.BackColor = BackColor;
        }

        protected virtual void SetCaptionButtons()
        {
            var buttonWidth = Padding.Top;
            var buttonHeight = Padding.Top - Padding.Bottom;

            _closeButton.Font = _buttonFont;
            _closeButton.Width = buttonWidth;
            _closeButton.Height = buttonHeight;
            _closeButton.Location = new Point(Width - buttonWidth - Padding.Right, Padding.Bottom);

            _restoreButton.Font = _buttonFont;
            _restoreButton.Width = buttonWidth;
            _restoreButton.Height = buttonHeight;
            _restoreButton.Location = new Point(_closeButton.Left - buttonWidth - Padding.Right, _closeButton.Top);

            _minimizeButton.Font = _buttonFont;
            _minimizeButton.Width = buttonWidth;
            _minimizeButton.Height = buttonHeight;
            _minimizeButton.Location = new Point(_restoreButton.Left - buttonWidth - Padding.Right, _closeButton.Top);
        }

        private Padding GetCaptionPadding()
        {
            if (WindowState == FormWindowState.Maximized)
                return new Padding(0);

            var captionSize = WindowsUtilities.GetWindowCaptionRect(Handle);
            var border = DpiUtilities.AdjustForWindowDpi(_frameSize, Handle);
            return new Padding(border, Math.Abs(captionSize.top), border, border);
        }

        private void Restore()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                WindowState = FormWindowState.Maximized;
            }
        }
    }
}
