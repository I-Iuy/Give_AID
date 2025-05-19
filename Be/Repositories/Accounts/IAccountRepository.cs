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
        Task<Account?> LoginAsync(string email, string password);
        Task<bool> SetAccountStatusAsync(int id, bool isActive);
    }
}
