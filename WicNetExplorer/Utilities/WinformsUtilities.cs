using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirectN;
using WicNet.Utilities;

namespace WicNetExplorer.Utilities
{
    public static class WinformsUtilities
    {
        public static string ApplicationName => Resources.AppName;
        public static string? ApplicationVersion => Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        public static string ApplicationTitle => ApplicationName + " V" + ApplicationVersion;

        public static void ShowMessage(this IWin32Window owner, string text) => MessageBox.Show(owner, text, ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        public static DialogResult ShowConfirm(this IWin32Window owner, string text) => MessageBox.Show(owner, text, ApplicationTitle + " - " + Resources.Confirmation, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        public static DialogResult ShowQuestion(this IWin32Window owner, string text) => MessageBox.Show(owner, text, ApplicationTitle + " - " + Resources.Confirmation, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        public static void ShowError(this IWin32Window owner, string text) => MessageBox.Show(owner, text, ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        public static void ShowWarning(this IWin32Window owner, string text) => MessageBox.Show(owner, text, ApplicationTitle + " - " + Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);

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

            var oldDpi = (int)primary.EffectiveDpi.width;
            var newDpi = (int)startup.EffectiveDpi.width;

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
