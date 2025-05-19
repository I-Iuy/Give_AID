using Be.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Be.Repositories.ContentPageRepositories 
{
    public interface IContentPageRepository
    {
        Task<List<ContentPage>> GetAllAsync();
        Task<ContentPage?> GetByIdAsync(int id);
        Task<ContentPage> CreateAsync(ContentPage contentPage);
        Task<ContentPage?> UpdateAsync(ContentPage contentPage);
        Task<bool> DeleteAsync(int id);
    }
}
