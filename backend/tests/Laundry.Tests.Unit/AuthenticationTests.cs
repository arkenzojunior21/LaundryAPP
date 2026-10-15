using Laundry.Domain.Identity;
using Xunit;

namespace Laundry.Tests.Unit;

public sealed class AuthenticationTests
{
    [Fact]
    public void PasswordHash_DoesNotExposePasswordAndCanBeVerified()
    {
        var hash = PasswordHashing.Hash("kata-sandi-aman");
        Assert.DoesNotContain("kata-sandi-aman", hash);
        Assert.True(PasswordHashing.Verify("kata-sandi-aman", hash));
        Assert.False(PasswordHashing.Verify("salah-password", hash));
    }

    [Fact]
    public void FiveFailedAttempts_LockAccountForFifteenMinutes()
    {
        var now = new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);
        var account = new UserAccount(Guid.NewGuid(), "628123456789", UserRole.Customer, PasswordHashing.Hash("kata-sandi-aman"));
        for (var attempt = 0; attempt < 5; attempt++) Assert.False(account.VerifyPassword("salah-password", now));
        Assert.Equal(now.AddMinutes(15), account.LockedUntilUtc);
        Assert.False(account.VerifyPassword("kata-sandi-aman", now.AddMinutes(1)));
        Assert.True(account.VerifyPassword("kata-sandi-aman", now.AddMinutes(16)));
    }
}
