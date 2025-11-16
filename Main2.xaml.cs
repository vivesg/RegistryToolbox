using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace RegistryToolbox
{
    /// <summary>
    /// Interaction logic for Main2.xaml
    /// </summary>
    public partial class Main2
    {
        public Main2()
        {
            InitializeComponent();
            ApplicationThemeManager.ApplySystemTheme();

        }

        // Remove the problematic line in the NavigationViewItem_Click method
        private void NavigationViewItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is NavigationViewItem navigationViewItem)
            {
                string pageName = navigationViewItem.Tag?.ToString();
                if (!string.IsNullOrEmpty(pageName))
                {
                    Type pageType = Type.GetType($"RegistryToolbox.Pages.{pageName}");
                    if (pageType != null)
                    {
                        // Fix: Pass the Type, not an instance, to Navigate
                        RootNavigation.Navigate(pageType);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light)
            {
                ApplicationThemeManager.Apply(
                ApplicationTheme.Dark,
                    WindowBackdropType.Mica
            );
                return;
            }
            ApplicationThemeManager.Apply(
                ApplicationTheme.Light,
                    WindowBackdropType.Mica
            );
        }
    }
}
