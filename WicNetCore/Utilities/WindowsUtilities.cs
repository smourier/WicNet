using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using DirectN;
using DirectNAot.Extensions.Utilities;
using Microsoft.Win32;

namespace WicNet.Utilities;

public static class WindowsUtilities
{
    private static readonly Lazy<Process> _currentProcess = new(() => Process.GetCurrentProcess(), true);
    public static Process CurrentProcess => _currentProcess.Value;

    private static readonly Lazy<Version> _kernelVersion = new(() =>
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

    [SupportedOSPlatform("windows")]
    public static int TextScaleFactor => _textScaleFactor.Value;

    [SupportedOSPlatform("windows")]
    private static readonly Lazy<int> _textScaleFactor = new(() =>
    {
        try
        {
            // https://stackoverflow.com/questions/64785427/c-windows-api-how-to-retrieve-font-scaling-percentage
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Accessibility", false);
            if (key != null)
            {
                if (Conversions.TryChangeType<int>(key.GetValue("TextScaleFactor"), out var factor) && factor > 0)
                    return factor;
            }
        }
        catch
        {
            // continue
        }
        return 100;
    }, true);

    // https://stackoverflow.com/a/61681245/403671
    [SupportedOSPlatform("windows10.0.14393")]
    public static RECT GetWindowCaptionRect(HWND handle) => GetWindowCaptionRect(DpiUtilities.GetDpiForWindow(handle).width);

    [SupportedOSPlatform("windows10.0.14393")]
    public static RECT GetWindowCaptionRect(uint dpi)
    {
        var rc = new RECT();
        if (KernelVersion >= new Version(10, 0))
        {
            Functions.AdjustWindowRectExForDpi(ref rc, WINDOW_STYLE.WS_OVERLAPPEDWINDOW, false, 0, dpi);
        }
        else
        {
            Functions.AdjustWindowRectEx(ref rc, WINDOW_STYLE.WS_OVERLAPPEDWINDOW, false, 0);
        }
        return rc;
    }
}
