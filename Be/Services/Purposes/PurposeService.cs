using Be.DTOs.Purposes;
using Be.Models;
using Be.Repositories.Purposes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Be.Services.Purposes
{
    public class PurposeService : IPurposeService
    {
        private readonly IPurposeRepository _repo;

        public PurposeService(IPurposeRepository repo)
        {
            _repo = repo;
        }

        // Lấy danh sách Purpose và chuyển sang DTO
        public async Task<IEnumerable<PurposeDto>> GetAllAsync()
        {
            var purposes = await _repo.GetAllAsync();

            return purposes.Select(p => new PurposeDto
            {
                PurposeId = p.PurposeId,
                Title = p.Title
            });
        }

        // Thêm Purpose mới (có kiểm tra trùng và trống)
        public async Task AddAsync(CreatePurposeDto dto)
        {
            // Kiểm tra rỗng hoặc toàn khoảng trắng
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ArgumentException("Title must not be empty or whitespace.");
            }

            // Kiểm tra trùng tiêu đề (không phân biệt hoa thường)
            var allPurposes = await _repo.GetAllAsync();
            bool isDuplicate = allPurposes.Any(p =>
                string.Equals(p.Title.Trim(), dto.Title.Trim(), StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
            {
                throw new ArgumentException("A purpose with the same title already exists.");
            }

            var purpose = new Purpose
            {
                Title = dto.Title.Trim(),
                AccountId = dto.AccountId
            };

            await _repo.AddAsync(purpose);
        }

        // Xoá Purpose theo ID
        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
