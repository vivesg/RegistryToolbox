namespace RegistryToolboxUI;

using RegistryToolboxUI.Views.Pages;
using Wpf.Ui.Appearance;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        DataContext = this;

        Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

        InitializeComponent();

        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(ApplicationTheme.Dark);

        Loaded += (_, _) => RootNavigation.Navigate(typeof(DashboardPage));
    }
}
