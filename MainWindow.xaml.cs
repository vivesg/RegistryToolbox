using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Registry;
using Registry.Abstractions;
using RegistryToolbox.Models;
using System.Threading;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using RegistryToolbox.Insights;
using System.Text;

namespace RegistryToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        
        public MainWindow()
        {
            InitializeComponent();

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 5, 0);
            dispatcherTimer.Start();

        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;//put here your logic
            e.Handled = true;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // code goes here
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software", true);
            key.CreateSubKey("RegistryToolbox");
            key = key.OpenSubKey("RegistryToolbox", true);
            key.SetValue("LaunchMode", "1" , Microsoft.Win32.RegistryValueKind.DWord);

            Main2 Registry2 = new Main2();
            Registry2.Show();
            this.Close();
        }

        private void btnManual_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOpenReg_Click(object sender, RoutedEventArgs e)
        {
            gridClientsContainer1.Visibility = Visibility.Visible;
        }

        private void btnCMPReg_Click(object sender, RoutedEventArgs e)
        {
            gridClientsContainer1.Visibility = Visibility.Visible;
        }
    }
}
