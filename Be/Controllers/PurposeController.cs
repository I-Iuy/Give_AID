using Be.DTOs.Purposes;
using Be.Models;
using Be.Services.CampaignUsage;
using Be.Services.Purposes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurposeController : ControllerBase
    {
        private readonly IPurposeService _service;
        private readonly ICampaignUsageService _usageService;
        public PurposeController(IPurposeService service, ICampaignUsageService usageService)
        {
            _service = service;
            _usageService = usageService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result); 
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
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
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
      
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpGet("{id}/is-used")]
        public async Task<IActionResult> IsPurposeUsed(int id)
        {
            bool isUsed = await _usageService.IsPurposeUsedAsync(id);
            return Ok(new { isUsed });
        }
        [HttpPut]
        public async Task<IActionResult> Edit([FromBody] UpdatePurposeDto dto)
        {
            try
            {
                await _service.EditAsync(dto);
                return Ok("Purpose updated successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok("Purpose deleted successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); 
            }
        }

    }
}
