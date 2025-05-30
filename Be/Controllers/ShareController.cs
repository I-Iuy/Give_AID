using Be.DTOs.Share;
using Be.Services.ShareService;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ShareController : ControllerBase
{
    private readonly IShareService _shareService;

    public ShareController(IShareService shareService)
    {
        _shareService = shareService;
    }

    [HttpPost]
    public async Task<IActionResult> ShareCampaign([FromBody] CreateShareDto dto)
    {
        try
        {
            if (dto.CampaignId <= 0)
                return BadRequest(new { message = "CampaignId is required." });

            var result = await _shareService.ShareAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while sharing the campaign.",
                details = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _shareService.GetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to retrieve share list.", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _shareService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = $"No share found with ID = {id}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving share data.", details = ex.Message });
        }
    }
}
