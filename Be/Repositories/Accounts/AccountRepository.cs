using Be.DTOs.Account;
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

    // Get all accounts
    public async Task<List<Account>> GetAllAsync()
        => await _context.Accounts.ToListAsync();

    // Get account by ID
    public async Task<Account?> GetByIdAsync(int id)
        => await _context.Accounts.FindAsync(id);

    // Create new account
    public async Task<Account> CreateAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    // Admin: update all account fields
    public async Task<Account?> UpdateAsync(Account account)
    {
        var acc = await _context.Accounts.FindAsync(account.AccountId);
        if (acc == null) return null;

        _context.Entry(acc).CurrentValues.SetValues(account);
        await _context.SaveChangesAsync();
        return acc;
    }

    // Delete account by ID
    public async Task<bool> DeleteAsync(int id)
    {
        var acc = await _context.Accounts.FindAsync(id);
        if (acc == null) return false;

        _context.Accounts.Remove(acc);
        await _context.SaveChangesAsync();
        return true;
    }

    // Login by email (no password check here, only active status)
    public async Task<Account?> LoginAsync(string email, string _)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == email && a.IsActive);
    }

    // Admin: set active/inactive status
    public async Task<bool> SetAccountStatusAsync(int id, bool isActive)
    {
        var acc = await _context.Accounts.FindAsync(id);
        if (acc == null) return false;

        acc.IsActive = isActive;
        await _context.SaveChangesAsync();
        return true;
    }

    // Get account by reset token (used in password reset)
    public async Task<Account?> GetByResetTokenAsync(string token)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.ResetToken == token && a.ResetTokenExpires > DateTime.UtcNow);
    }

    // Set password reset token and expiry
    public async Task<bool> SetResetTokenAsync(string email, string token, DateTime expires)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        if (account == null) return false;

        account.ResetToken = token;
        account.ResetTokenExpires = expires;
        await _context.SaveChangesAsync();
        return true;
    }

    // Reset password by token
    public async Task<bool> ResetPasswordAsync(string token, string newHashedPassword)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.ResetToken == token && a.ResetTokenExpires > DateTime.UtcNow);
        if (account == null) return false;

        account.Password = newHashedPassword;
        account.ResetToken = null;
        account.ResetTokenExpires = null;
        await _context.SaveChangesAsync();
        return true;
    }

    // User updates their own profile information
    public async Task<bool> UpdateAccountInfoAsync(int id, AccountUpdateDto dto)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null || !account.IsActive) return false;

        account.FullName = dto.FullName;
        account.DisplayName = dto.DisplayName;
        account.Phone = dto.Phone;
        account.Address = dto.Address;
        await _context.SaveChangesAsync();
        return true;
    }

    // User changes password with current password verification
    public async Task<int> ChangePasswordAsync(int id, string currentPassword, string newHashedPassword)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null || !account.IsActive) return 0;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, account.Password))
            return 1; // current password incorrect

        account.Password = newHashedPassword;
        await _context.SaveChangesAsync();
        return 2; // success
    }

    // Get account by email (used in registration validation and password reset)
    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
    }
}
