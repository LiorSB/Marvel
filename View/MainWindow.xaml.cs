﻿using Marvel.Model;
using Marvel.ViewModel;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Marvel.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _viewModel.OnHostAdded -= _viewModel_OnHostAdded;
            _viewModel.OnHostAdded += _viewModel_OnHostAdded;
        }

        private void _viewModel_OnHostAdded(object sender, System.EventArgs e)
        {
            HostPasswordBox.Password = string.Empty;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                PasswordBox password = (PasswordBox)sender;
                _viewModel.Password = password.Password;
            }
        }

        private void Button_EditHost(object sender, RoutedEventArgs e)
        {
            EditHostWindow editHostWindow = new();
            editHostWindow.DataContext = new EditHostViewModel((Host)((Button)sender).DataContext);
            editHostWindow.Owner = this;
            editHostWindow.ShowDialog();
        }

        private void Button_AddHostsFromFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();

            if (openFileDialog.ShowDialog() == true)
            {
                _viewModel.HostsFileDirectory = openFileDialog.FileName;
            }
        }
    }
}