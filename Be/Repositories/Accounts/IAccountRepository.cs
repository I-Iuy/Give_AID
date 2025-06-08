using Be.DTOs.Account;
using Be.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Be.Repositories.Accounts
{
    public interface IAccountRepository
    {
        Task<List<Account>> GetAllAsync();
        Task<Account?> GetByIdAsync(int id);
        Task<Account> CreateAsync(Account account);
        Task<Account?> UpdateAsync(Account account);
        Task<bool> UpdateAccountInfoAsync(int id, AccountUpdateDto dto);
        Task<Account?> LoginAsync(string email, string password);
        Task<Account?> GetByEmailAsync(string email);
        Task<bool> SetAccountStatusAsync(int id, bool isActive);
        Task<Account?> GetByResetTokenAsync(string token);
        Task<bool> SetResetTokenAsync(string email, string token, DateTime expires);
        Task<bool> ResetPasswordAsync(string token, string newHashedPassword);

        Task<int> ChangePasswordAsync(int id, string currentPassword, string newHashedPassword);

    }
}
