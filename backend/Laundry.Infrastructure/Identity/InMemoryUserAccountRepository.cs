using System.Collections.Concurrent;
using Laundry.Application.Identity;
using Laundry.Domain.Identity;

namespace Laundry.Infrastructure.Identity;

/// <summary>Development-only store. Production must persist credentials in PostgreSQL.</summary>
public sealed class InMemoryUserAccountRepository : IUserAccountRepository
{
    private readonly ConcurrentDictionary<string, UserAccount> _accounts = new(StringComparer.Ordinal);
    public Task<UserAccount?> FindByWhatsAppAsync(string whatsappNumber, CancellationToken cancellationToken) => Task.FromResult(_accounts.GetValueOrDefault(whatsappNumber.Trim()));
    public Task AddAsync(UserAccount account, CancellationToken cancellationToken) { _accounts[account.WhatsAppNumber] = account; return Task.CompletedTask; }
}
