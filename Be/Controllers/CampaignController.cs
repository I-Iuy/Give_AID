using Be.DTOs.Campaigns;
using Be.Services.Campaigns;
using Microsoft.AspNetCore.Mvc;

namespace Be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignController : Controller
    {
        private readonly ICampaignService _service;
        public CampaignController(ICampaignService service)
        {
            _service = service;
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
        public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
        {
            try
            {
                await _service.AddAsync(dto);
                return Ok("Campaign created successfully.");
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
        [HttpPut]
        public async Task<IActionResult> Edit([FromBody] UpdateCampaignDto dto)
        {
            try
            {
                await _service.EditAsync(dto);
                return Ok("Campaign updated successfully.");
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
                return Ok("Campaign deleted successfully.");
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
    }
}
