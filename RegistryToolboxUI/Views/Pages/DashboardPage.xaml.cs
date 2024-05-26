// <copyright file="DashboardPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>


namespace RegistryToolboxUI.Views.Pages; 

using System.Windows;
/// <summary>
/// Interaction logic for DashboardPage.xaml
/// </summary>
public partial class DashboardPage
{
    private int _counter = 0;

    public DashboardPage()
    {
        DataContext = this;
        InitializeComponent();

       
    }

    private void OnBaseButtonClick(object sender, RoutedEventArgs e)
    {
       
    }
}

