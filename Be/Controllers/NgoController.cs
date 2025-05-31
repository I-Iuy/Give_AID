using Be.Dtos.Ngos;
using Be.Services.Ngos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NgoController : ControllerBase
    {
        private readonly INgoService _service;

        public NgoController(INgoService service)
        {
            _service = service;
        }

        // GET: api/ngo
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // GET: api/ngo/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // POST: api/ngo
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

        // PUT: api/ngo
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

        // DELETE: api/ngo/{id}
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
