using System.Windows;
using System.Windows.Controls;

namespace Marvel
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
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                PasswordBox password = (PasswordBox)sender;
                _viewModel.Password = password.Password;
                //password.Password = string.Empty;
            }
        }

        private void Button_EditHost(object sender, RoutedEventArgs e)
        {
            EditHostWindow editHostWindow = new();
            editHostWindow.Show();
        }
    }
}