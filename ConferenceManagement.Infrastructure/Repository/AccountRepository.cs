using ConferenceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceManagement.Infrastructure.Repository;

public sealed class AccountRepository(Context context)
{
    public Task<ConferenceAccount?> FindAsync(string login) =>
        context.Accounts.SingleOrDefaultAsync(account => account.Login == login);

    public Task<ConferenceAccount?> GetAsync(Guid id) =>
        context.Accounts.FindAsync(id).AsTask();

    public Task<bool> HasOrganizerAsync() =>
        context.Accounts.AnyAsync(account => account.IsOrganizer);

    public async Task AddAsync(ConferenceAccount account)
    {
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
    }

    public Task SaveAsync() => context.SaveChangesAsync();
}
