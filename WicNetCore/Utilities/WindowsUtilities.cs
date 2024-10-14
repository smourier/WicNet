namespace WicNet.Utilities;

public static class WindowsUtilities
{
    private static readonly Lazy<Process> _currentProcess = new(Process.GetCurrentProcess, true);
    public static Process CurrentProcess => _currentProcess.Value;
}
