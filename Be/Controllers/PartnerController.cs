using Be.DTOs.Partners;
using Be.Services.Partners;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartnerController : ControllerBase
    {
        private readonly IPartnerService _service;

        public PartnerController(IPartnerService service)
        {
            _service = service;
        }

        // GET: api/partner
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // GET: api/partner/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // POST: api/partner
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePartnerDto dto)
        {
            try
            {
                await _service.AddAsync(dto);
                return Ok("Partner created successfully.");
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

        // PUT: api/partner
        [HttpPut]
        public async Task<IActionResult> Edit([FromBody] UpdatePartnerDto dto)
        {
            try
            {
                await _service.EditAsync(dto);
                return Ok("Partner updated successfully.");
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

        // DELETE: api/partner/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok("Partner deleted successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
