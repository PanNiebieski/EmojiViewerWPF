using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace EmojiViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow
        {
            Title = "Emoji Viewer",
            Width = 1000,
            Height = 700,
            Background = new SolidColorBrush(Color.FromRgb(32, 32, 32)),
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            WindowStyle = WindowStyle.None,        // remove default chrome
            AllowsTransparency = false,
            BorderThickness = new Thickness(0),
            ResizeMode = ResizeMode.NoResize

            // keep performance (true = full transparency support but slower)
            //Background = new SolidColorBrush(Color.FromRgb(32, 32, 32)), // black window
        };

        mainWindow.Show();
    }
}
