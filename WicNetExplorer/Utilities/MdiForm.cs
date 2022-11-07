using System;
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
        private readonly Button _closeButton = new() { Text = "r", FlatStyle = FlatStyle.Flat, Name = "Close" };
        private readonly Button _restoreButton = new() { Text = "1", FlatStyle = FlatStyle.Flat, Name = "Restore" };
        private readonly Button _minimizeButton = new() { Text = "0", FlatStyle = FlatStyle.Flat, Name = "Minimize" };

        public MdiForm()
        {
            SuspendLayout();
            FormBorderStyle = FormBorderStyle.None; // we draw most of it ourselves
            AutoScaleMode = AutoScaleMode.Font; // dpi support
            Name = "MdiForm";
            Text = Name;

            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.Click += (s, e) => Close();
            Controls.Add(_closeButton);

            _restoreButton.FlatAppearance.BorderSize = _closeButton.FlatAppearance.BorderSize;
            _restoreButton.Click += (s, e) => Restore();
            Controls.Add(_restoreButton);

            _minimizeButton.FlatAppearance.BorderSize = _closeButton.FlatAppearance.BorderSize;
            _minimizeButton.Click += (s, e) => WindowState = FormWindowState.Minimized;
            Controls.Add(_minimizeButton);

            ResumeLayout(false);
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

            // emulate frame sizing & caption move
            if (m.Msg == MessageDecoder.WM_NCHITTEST)
            {
                var screenLeftTop = Parent.PointToScreen(new Point(Left, Top));
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
                using (var brush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(brush, Padding.Left, Padding.Bottom, Width - Padding.Horizontal, height);
                }

                var size = TextRenderer.MeasureText(Text, _textFont);
                var maxWidth = _minimizeButton.Left;
                TextRenderer.DrawText(e.Graphics, Text, _textFont, new Rectangle(Padding.Left, (height - size.Height) / 2, Math.Min(size.Width, maxWidth), size.Height), ForeColor);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecomputeSizes();
            InvalidateCaption();
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

        private void RecomputeSizes()
        {
            SetFonts();
            Padding = GetCaptionPadding();
            SetCaptionButtons();
        }

        private void SetFonts()
        {
            _initialFontSize ??= Font.Size;
            var fontSize = DpiUtilities.AdjustForWindowDpi(_initialFontSize.Value, Handle);

            _textFont?.Dispose();
            _textFont = new Font(Font.FontFamily.Name, fontSize);

            _buttonFont?.Dispose();
            _buttonFont = new Font("Marlett", fontSize);
        }

        private void SetCaptionButtons()
        {
            var buttonWidth = Padding.Top;
            var buttonHeight = Padding.Top - Padding.Bottom * 2;

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

            var dpi = DpiUtilities.GetDpiForWindow(Handle);
            var captionSize = WindowsUtilities.GetWindowCaptionRect(dpi);
            var border = DpiUtilities.AdjustForWindowDpi(4, Handle);
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
