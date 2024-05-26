// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.


namespace RegistryToolboxUI.Views.Pages;

using System;
using System.Windows;
using Wpf.Ui.Appearance;
/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage
{
    public SettingsPage()
    {
        InitializeComponent();

        AppVersionTextBlock.Text = $"WPF UI - Simple Demo - {GetAssemblyVersion()}";

        if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark)
        {
            DarkThemeRadioButton.IsChecked = true;
        }
        else
        {
            LightThemeRadioButton.IsChecked = true;
        }
    }

    private void OnLightThemeRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(ApplicationTheme.Light);
    }

    private void OnDarkThemeRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(ApplicationTheme.Dark);
    }

    private static string GetAssemblyVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? string.Empty;
    }
}
