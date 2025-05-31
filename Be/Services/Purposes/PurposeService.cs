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

        private readonly DatabaseContext _context;

        public PurposeService(IPurposeRepository repo, DatabaseContext context)
        {
            _repo = repo;
            _context = context;
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
        // Lấy Purpose theo ID và chuyển sang DTO
        public async Task<PurposeDto> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            return new PurposeDto
            {
                PurposeId = p.PurposeId,
                Title = p.Title
            };
        }
        private bool IsTitleValid(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title must not be empty or whitespace.");
            return true;
        }
        private async Task<bool> IsTitleUnique(string title, int? excludeId = null)
        {
            var all = await _repo.GetAllAsync();
            bool exists = all.Any(p =>
                (excludeId == null || p.PurposeId != excludeId) &&
                string.Equals(p.Title.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
                throw new ArgumentException("A purpose with the same title already exists.");
            return true;
        }

        // Thêm Purpose mới (có kiểm tra trùng và trống)
        public async Task AddAsync(CreatePurposeDto dto)
        {
            IsTitleValid(dto.Title);
            await IsTitleUnique(dto.Title);
            var purpose = new Purpose
            {
                Title = dto.Title.Trim(),
                AccountId = dto.AccountId
            };

            await _repo.AddAsync(purpose);
        }
        public async Task EditAsync(UpdatePurposeDto dto)
        {
            IsTitleValid(dto.Title);
            await IsTitleUnique(dto.Title, dto.PurposeId);
            var existing = await _repo.GetByIdAsync(dto.PurposeId);
            if (existing == null)
                throw new ArgumentException("Purpose not found.");

            existing.Title = dto.Title.Trim();
            existing.AccountId = dto.AccountId;
            await _repo.EditAsync(existing);
        }
        // Xoá Purpose theo ID
        public async Task DeleteAsync(int id)
        {
            // Kiểm tra có campaign nào đang dùng purpose này không
            bool isUsed = _context.Campaigns.Any(c => c.PurposeId == id);
            if (isUsed)
            {
                throw new InvalidOperationException("Purpose is in use. Delete related campaigns first.");
            }

            await _repo.DeleteAsync(id);
        }

    }
}
