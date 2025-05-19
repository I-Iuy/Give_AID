using Be.Models;
using Be.Repositories.Accounts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;


public class AccountRepository : IAccountRepository
{
    private readonly DatabaseContext _context;

    public AccountRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetAllAsync()
        => await _context.Accounts.ToListAsync();

    public async Task<Account?> GetByIdAsync(int id)
        => await _context.Accounts.FindAsync(id);

    public async Task<Account> CreateAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<Account?> UpdateAsync(Account account)
    {
        var acc = await _context.Accounts.FindAsync(account.AccountId);
        if (acc == null) return null;

        _context.Entry(acc).CurrentValues.SetValues(account);
        await _context.SaveChangesAsync();
        return acc;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var acc = await _context.Accounts.FindAsync(id);
        if (acc == null) return false;

        _context.Accounts.Remove(acc);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Account?> LoginAsync(string email, string password)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == email && a.IsActive);
    }

    public async Task<bool> SetAccountStatusAsync(int id, bool isActive)
    {
        var acc = await _context.Accounts.FindAsync(id);
        if (acc == null) return false;

        acc.IsActive = isActive;
        await _context.SaveChangesAsync();
        return true;
    }
}
