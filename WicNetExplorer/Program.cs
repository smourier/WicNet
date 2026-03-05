using System;
using System.Windows.Forms;

namespace WicNetExplorer;

public static class Program
{
    public static DirectN.Extensions.Utilities.WindowsDispatcherQueueController DispatcherQueueController { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        DispatcherQueueController = new DirectN.Extensions.Utilities.WindowsDispatcherQueueController();
        DispatcherQueueController.EnsureOnCurrentThread();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.Run(new Main());
    }
}