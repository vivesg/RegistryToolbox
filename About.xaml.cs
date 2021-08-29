using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace RegistryToolbox
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            string version = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + '.'
                       + Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString()
                       + '.' + Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
            lblversion.Content = "Registry Toolbox V" + version;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR4t8jW939JRFosWxHm34rHRUNVEzVjhaUk4zQVQxM1hSNU82SkgxS1JFQS4u");
        }
    }
}
