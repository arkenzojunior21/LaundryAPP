using Laundry.Domain.Identity;

namespace Laundry.Application.Identity;

public sealed record RegisterCustomer(string WhatsAppNumber, string Password);
public sealed record LoginRequest(string WhatsAppNumber, string Password);

public interface IUserAccountRepository
{
    Task<UserAccount?> FindByWhatsAppAsync(string whatsappNumber, CancellationToken cancellationToken);
    Task AddAsync(UserAccount account, CancellationToken cancellationToken);
}

public sealed class AuthenticationService(IUserAccountRepository accounts, TimeProvider clock)
{
    public async Task<UserAccount> RegisterCustomerAsync(RegisterCustomer request, CancellationToken cancellationToken)
    {
        if (await accounts.FindByWhatsAppAsync(request.WhatsAppNumber, cancellationToken) is not null) throw new InvalidOperationException("Nomor WhatsApp sudah terdaftar.");
        var account = new UserAccount(Guid.NewGuid(), request.WhatsAppNumber.Trim(), UserRole.Customer, PasswordHashing.Hash(request.Password));
        await accounts.AddAsync(account, cancellationToken);
        return account;
    }

    public async Task<UserAccount?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var account = await accounts.FindByWhatsAppAsync(request.WhatsAppNumber, cancellationToken);
        return account is not null && account.VerifyPassword(request.Password, clock.GetUtcNow()) ? account : null;
    }
}
