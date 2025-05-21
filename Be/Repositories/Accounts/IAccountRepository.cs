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
        Task<bool> SetAccountStatusAsync(int id, bool isActive);
        Task<int> ChangePasswordAsync(int id, string currentPassword, string newHashedPassword);

    }
}
