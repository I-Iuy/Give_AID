using Be.DTOs.Purposes;
using Be.Services.Purposes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurposeController : ControllerBase
    {
        private readonly IPurposeService _service;

        public PurposeController(IPurposeService service)
        {
            _service = service;
        }

        // GET: api/purpose
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result); // Trả về danh sách dạng JSON
        }

        // POST: api/purpose
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurposeDto dto)
        {
            try
            {
                await _service.AddAsync(dto);
                return Ok("Purpose created successfully.");
            }
            catch (ArgumentException ex)
            {
                // Trả lỗi nghiệp vụ ra FE (giao diện tiếng Anh)
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                // Tránh leak lỗi hệ thống
                return StatusCode(500, "An unexpected error occurred.");
            }
        }


        // DELETE: api/purpose/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok("Purpose deleted successfully.");
        }
    }
}
