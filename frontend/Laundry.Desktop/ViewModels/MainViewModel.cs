using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Laundry.Desktop.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private bool _isAuthenticated;
    private string _role = string.Empty;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ICommand LoginAdminCommand => new DelegateCommand(() => Login("Admin"));
    public ICommand LoginCustomerCommand => new DelegateCommand(() => Login("Pelanggan"));
    public ICommand LockCommand => new DelegateCommand(() => { _isAuthenticated = false; _role = string.Empty; NotifyAll(); });
    public Visibility LoginVisibility => _isAuthenticated ? Visibility.Collapsed : Visibility.Visible;
    public Visibility DashboardVisibility => _isAuthenticated ? Visibility.Visible : Visibility.Collapsed;
    public string DashboardTitle => $"Dashboard {_role}";
    public string DashboardSummary => _role == "Admin" ? "Pesanan baru, proses laundry, pembayaran tunai, dan tindakan sinkronisasi akan tersedia di sini." : "Pesanan aktif, tagihan, saldo kredit, dan jadwal antar-jemput akan tersedia di sini.";
    public string SyncStatus => _role == "Admin" ? "Status sinkronisasi: belum dikonfigurasi — operasi lokal akan memakai PostgreSQL pada fase persistence." : "Memerlukan koneksi internet untuk data cloud.";
    private void Login(string role) { _role = role; _isAuthenticated = true; NotifyAll(); }
    private void NotifyAll() { foreach (var property in new[] { nameof(LoginVisibility), nameof(DashboardVisibility), nameof(DashboardTitle), nameof(DashboardSummary), nameof(SyncStatus) }) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)); }
}

public sealed class DelegateCommand(Action execute) : ICommand
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => execute();
}
