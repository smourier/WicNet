using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using DirectN;
using Microsoft.Win32;

namespace WicNet.Utilities
{
    public static class WindowsUtilities
    {
        private static readonly Lazy<Process> _currentProcess = new Lazy<Process>(() => Process.GetCurrentProcess(), true);
        public static Process CurrentProcess => _currentProcess.Value;

        private static readonly Lazy<Version> _kernelVersion = new Lazy<Version>(() =>
        {
            // warning: this requires a manifest with Windows 10 marked, like this
            //
            //<compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
            //  <application>
            //    <!-- Windows 10 -->
            //    <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
            //  </application>
            //</compatibility>
            //

            var vi = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, "kernel32.dll"));
            return new Version(vi.FileMajorPart, vi.FileMinorPart, vi.FileBuildPart, vi.FilePrivatePart);
        }, true);

        public static Version KernelVersion => _kernelVersion.Value;
        private static readonly Lazy<int> _textScaleFactor = new Lazy<int>(() =>
        {
            try
            {
                // https://stackoverflow.com/questions/64785427/c-windows-api-how-to-retrieve-font-scaling-percentage
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Accessibility", false))
                {
                    if (key != null)
                    {
                        if (Conversions.TryChangeType<int>(key.GetValue("TextScaleFactor"), out var factor) && factor > 0)
                            return factor;
                    }
                }
            }
            catch
            {
                // continue
            }
            return 100;
        }, true);
        public static int TextScaleFactor => _textScaleFactor.Value;

        // https://stackoverflow.com/a/61681245/403671
        public static tagRECT GetWindowCaptionRect(IntPtr handle) => GetWindowCaptionRect((int)DpiUtilities.GetDpiForWindow(handle).width);
        public static tagRECT GetWindowCaptionRect(int dpi)
        {
            var rc = new tagRECT();
            if (KernelVersion >= new Version(10, 0))
            {
                AdjustWindowRectExForDpi(ref rc, WS_OVERLAPPEDWINDOW, false, 0, dpi);
            }
            else
            {
                AdjustWindowRectEx(ref rc, WS_OVERLAPPEDWINDOW, false, 0);
            }
            return rc;
        }

        private const int WS_OVERLAPPEDWINDOW = 0x00CF0000;

        [DllImport("user32", SetLastError = true)]
        private static extern bool AdjustWindowRectEx(ref tagRECT lpRect, int dwStyle, bool bMenu, int dwExStyle);

        [DllImport("user32", SetLastError = true)]
        private static extern bool AdjustWindowRectExForDpi(ref tagRECT lpRect, int dwStyle, bool bMenu, int dwExStyle, int dpi);
    }
}
