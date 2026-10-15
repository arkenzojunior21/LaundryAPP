using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Laundry.Desktop.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly HttpClient _client = new() { BaseAddress = new Uri("http://127.0.0.1:5088") };
    private bool _isAuthenticated;
    private bool _darkMode;
    private string _role = string.Empty;
    private string _whatsAppNumber = string.Empty;
    private string _password = string.Empty;
    private string _statusMessage = "Masuk menggunakan akun pelanggan yang sudah terdaftar.";

    public event PropertyChangedEventHandler? PropertyChanged;
    public ICommand LoginCustomerCommand => new AsyncDelegateCommand(LoginAsync, () => CanSubmit);
    public ICommand LockCommand => new DelegateCommand(Lock);
    public ICommand ToggleThemeCommand => new DelegateCommand(ToggleTheme);
    public string WhatsAppNumber { get => _whatsAppNumber; set { _whatsAppNumber = value; Notify(); Notify(nameof(CanSubmit)); } }
    public string Password { get => _password; set { _password = value; Notify(); Notify(nameof(CanSubmit)); } }
    public bool CanSubmit => !string.IsNullOrWhiteSpace(WhatsAppNumber) && !string.IsNullOrWhiteSpace(Password);
    public string StatusMessage { get => _statusMessage; private set { _statusMessage = value; Notify(); } }
    public Visibility LoginVisibility => _isAuthenticated ? Visibility.Collapsed : Visibility.Visible;
    public Visibility DashboardVisibility => _isAuthenticated ? Visibility.Visible : Visibility.Collapsed;
    public string DashboardTitle => $"Dashboard {_role}";
    public string DashboardSummary => _role == "Admin" ? "Pesanan baru, proses laundry, pembayaran tunai, dan tindakan sinkronisasi tersedia dari API." : "Pesanan aktif, tagihan, saldo kredit, dan jadwal antar-jemput akan tampil setelah modul pelanggan dihubungkan.";
    public string SyncStatus => _role == "Admin" ? "Status sinkronisasi: menunggu konfigurasi PostgreSQL lokal." : "Mode pelanggan memerlukan koneksi API.";

    private async Task LoginAsync()
    {
        StatusMessage = "Memverifikasi akun…";
        try
        {
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { whatsAppNumber = WhatsAppNumber, password = Password });
            if (!response.IsSuccessStatusCode) { StatusMessage = response.StatusCode == System.Net.HttpStatusCode.Unauthorized ? "Nomor atau password salah, atau akun sedang terkunci." : "Tidak dapat masuk. Periksa API dan data akun."; return; }
            var account = await response.Content.ReadFromJsonAsync<LoginResponse>();
            _role = account?.Role ?? "Pelanggan";
            _isAuthenticated = true;
            StatusMessage = "Login berhasil.";
            NotifyDashboard();
        }
        catch (HttpRequestException) { StatusMessage = "API belum berjalan. Jalankan Laundry.Api terlebih dahulu."; }
    }

    private void Lock() { _isAuthenticated = false; _role = string.Empty; Password = string.Empty; StatusMessage = "Aplikasi dikunci. Masuk kembali untuk melanjutkan."; NotifyDashboard(); }
    private void ToggleTheme()
    {
        _darkMode = !_darkMode;
        if (Application.Current?.MainWindow is not null) Application.Current.MainWindow.Background = _darkMode ? new SolidColorBrush(Color.FromRgb(31, 41, 55)) : Brushes.White;
    }
    private void NotifyDashboard() { foreach (var property in new[] { nameof(LoginVisibility), nameof(DashboardVisibility), nameof(DashboardTitle), nameof(DashboardSummary), nameof(SyncStatus) }) Notify(property); }
    private void Notify([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private sealed record LoginResponse(string Role);
}

public sealed class DelegateCommand(Action execute) : ICommand
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => execute();
}

public sealed class AsyncDelegateCommand(Func<Task> execute, Func<bool> canExecute) : ICommand
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => canExecute();
    public async void Execute(object? parameter) => await execute();
}
