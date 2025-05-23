using Fe.DTOs.Partners;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fe.Services.Partners
{
    public interface IPartnerApiService
    {
        Task<IEnumerable<PartnerDto>> GetAllAsync();
        Task<PartnerDto> GetByIdAsync(int id);
        FileStream GetLogoFileStream(string logoUrl);
        FileStream GetContractFileStream(string contractFileUrl);
        Task AddAsync(CreatePartnerDto dto, IFormFile logo, IFormFile contract);
        Task EditAsync(UpdatePartnerDto dto, IFormFile logo, IFormFile contract);
        Task DeleteAsync(int id);
    }
}
