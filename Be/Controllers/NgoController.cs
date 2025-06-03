using Be.Dtos.Ngos;
using Be.Services.CampaignUsage;
using Be.Services.Ngos;
using Microsoft.AspNetCore.Mvc;

namespace Be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NgoController : ControllerBase
    {
        private readonly INgoService _service;
        private readonly ICampaignUsageService _usageService;

        public NgoController(INgoService service, ICampaignUsageService usageService)
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
        public async Task<IActionResult> Create([FromBody] CreateNgoDto dto)
        {
            try
            {
                await _service.AddAsync(dto);
                return Ok("NGO created successfully.");
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
        public async Task<IActionResult> IsNgoUsed(int id)
        {
            bool isUsed = await _usageService.IsNgoUsedAsync(id);
            return Ok(new { isUsed });
        }
        [HttpPut]
        public async Task<IActionResult> Edit([FromBody] UpdateNgoDto dto)
        {
            try
            {
                await _service.EditAsync(dto);
                return Ok("NGO updated successfully.");
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
                return Ok("NGO deleted successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
