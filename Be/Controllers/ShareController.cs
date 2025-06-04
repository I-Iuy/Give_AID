using Be.DTOs.Share;
using Be.Services.ShareService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class ShareController : ControllerBase
{
    private readonly Be.Services.ShareService.IShareService _shareService;
    private readonly ILogger<ShareController> _logger;

    public ShareController(Be.Services.ShareService.IShareService shareService, ILogger<ShareController> logger)
    {
        _shareService = shareService;
        _logger = logger;
    }

    [HttpPost("sharecampaign")]
    public async Task<IActionResult> ShareCampaign([FromBody] CreateShareDto dto)
    {
        try
        {
            _logger.LogInformation("Received share request for campaign {CampaignId}", dto.CampaignId);

            if (!dto.CampaignId.HasValue || dto.CampaignId.Value <= 0)
            {
                _logger.LogWarning("Invalid campaign ID: {CampaignId}", dto.CampaignId);
                return BadRequest(new { message = "Invalid campaign ID." });
            }

            var result = await _shareService.ShareAsync(dto, dto.BaseUrl);
            _logger.LogInformation("Successfully shared campaign {CampaignId}", dto.CampaignId);

            return Ok(new { 
                message = "Campaign shared successfully!",
                data = result 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing campaign {CampaignId}", dto?.CampaignId);
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
            _logger.LogError(ex, "Error getting all shares");
            return StatusCode(500, new { 
                message = "Could not retrieve share list.", 
                details = ex.Message 
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _shareService.GetByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Share with ID {Id} not found", id);
                return NotFound(new { message = $"Share with ID {id} not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting share by ID {Id}", id);
            return StatusCode(500, new { 
                message = "Error retrieving share information.", 
                details = ex.Message 
            });
        }
    }
}
