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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegistryToolbox.Pages
{
    /// <summary>
    /// Interaction logic for LoadRegistryPage.xaml
    /// </summary>
    public partial class LoadRegistryPage : Page
    {
        public LoadRegistryPage()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                var window = Window.GetWindow(this); // Get the parent Window
                if (window != null)
                {
                    Wpf.Ui.Appearance.SystemThemeWatcher.Watch(
                        window,                                 // Pass Window instance
                        Wpf.Ui.Controls.WindowBackdropType.Acrylic,
                        true
                    );
                }
            };
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
