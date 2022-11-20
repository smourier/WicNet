using System;
using System.Windows.Forms;
using WicNetExplorer.Utilities;

namespace WicNetExplorer
{
    public static class Program
    {
        public static bool ForceWindows7Mode { get; } = CommandLine.GetArgument("w7", false);

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.Run(new Main());
        }
    }
}