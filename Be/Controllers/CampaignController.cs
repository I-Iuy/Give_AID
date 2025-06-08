using System.Net.Http.Headers;
using Be.DTOs.Campaigns;
using Be.Services.Campaigns;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        [HttpPost("suggest-content")]
        public async Task<IActionResult> SuggestContent([FromBody] string prompt)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sk-or-v1-..."); // thay bằng cấu hình

                var request = new
                {
                    model = "openai/gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "Bạn là một chuyên gia viết nội dung cho các chiến dịch từ thiện." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 500,
                    temperature = 0.7
                };

                var json = JsonConvert.SerializeObject(request);
                var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions",
                    new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return BadRequest(error);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseContent);
                string content = result.choices[0].message.content;

                return Ok(content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"AI Error: {ex.Message}");
            }
        }
    }
}
