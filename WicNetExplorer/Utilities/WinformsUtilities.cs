using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirectN;
using WicNet.Utilities;

namespace WicNetExplorer.Utilities
{
    public static class WinformsUtilities
    {
        // see https://developercommunity.visualstudio.com/content/problem/262330/high-dpi-support-in-windows-forms.html
        public static bool EnsureStartupDpi(this Form form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            if (form.Handle == IntPtr.Zero)
                return false;

            var startup = Monitor.FromWindow(form.Handle, MFW.MONITOR_DEFAULTTONEAREST);
            if (startup == null || startup.IsPrimary)
                return false;

            var primary = Monitor.Primary;
            if (primary == null || primary.EffectiveDpi == startup.EffectiveDpi)
                return false;

            var oldDpi = primary.EffectiveDpi;
            var newDpi = startup.EffectiveDpi;

            var rc = new tagRECT();
            var monitorRc = startup.Bounds;

            if (form.IsMdiChild)
            {
                rc.right = toDips(form.Width);
                rc.bottom = toDips(form.Height);
            }
            else
            {
                var left = form.Location.X;
                var top = form.Location.Y;
                rc.left = monitorRc.left + fromDips(left);
                rc.top = monitorRc.top + fromDips(top);
                rc.right = rc.left + toDips(form.Width);
                rc.bottom = rc.top + toDips(form.Height);
            }

            var p = (newDpi << 16) | newDpi;
            SendMessage(form.Handle, MessageDecoder.WM_DPICHANGED, (IntPtr)p, ref rc);
            return true;

            int toDips(int x) => newDpi * x / oldDpi;
            int fromDips(int x) => (oldDpi * x) / newDpi;
        }

        [DllImport("user32")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref tagRECT lParam);
    }
}
