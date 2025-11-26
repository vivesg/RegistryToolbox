using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RegistryToolbox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Fix: Use Registry.LocalMachine instead of Registry.LocalMachine (should be Microsoft.Win32.Registry.LocalMachine)

            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software", true);

            key.CreateSubKey("RegistryToolbox");
            key = key.OpenSubKey("RegistryToolbox", true);


            int value = (int)key.GetValue("LaunchMode", 0);
            if (value == 1)
            {
                Main2 newMainWindow = new Main2();
                this.MainWindow = newMainWindow;
                newMainWindow.ShowActivated = true;
                newMainWindow.Show();
            }
            else
            {
                // Create and show the new main window
                MainWindow newMainWindow = new MainWindow();
                this.MainWindow = newMainWindow;
                newMainWindow.Show();
            }


        }
    }
}
