using Microsoft.UI.Xaml;

namespace WicNet.WinUI3Tests;

public partial class App : Application
{
    private Window _window;

    public App() => InitializeComponent();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
