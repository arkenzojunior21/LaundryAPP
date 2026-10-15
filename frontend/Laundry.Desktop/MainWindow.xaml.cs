namespace Laundry.Desktop;

public partial class MainWindow : System.Windows.Window
{
    public MainWindow() { InitializeComponent(); DataContext = new ViewModels.MainViewModel(); }
    private void PasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel viewModel && sender is System.Windows.Controls.PasswordBox passwordBox) viewModel.Password = passwordBox.Password;
    }
}
