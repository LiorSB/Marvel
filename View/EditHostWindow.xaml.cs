using Marvel.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Marvel.View
{
    /// <summary>
    /// Interaction logic for EditHostWindow.xaml
    /// </summary>
    public partial class EditHostWindow : Window
    {
        private EditHostViewModel _editHostViewModel;
        public EditHostWindow()
        {
            InitializeComponent();

            DataContextChanged -= EditHostWindow_DataContextChanged;
            DataContextChanged += EditHostWindow_DataContextChanged;
        }

        private void EditHostWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _editHostViewModel = (EditHostViewModel)DataContext;
            EditPasswordBox.Password = _editHostViewModel.Host.Password;
            _editHostViewModel.RequestClose += (o, e) => { this.Close(); };
        }

        private void PasswordBox_HostPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                PasswordBox password = (PasswordBox)sender;
                _editHostViewModel.PasswordEdited = password.Password;
            }
        }
    }
}
