using Microsoft.AspNetCore.Mvc;
using WhatsappBot.Models.DTO;
using WhatsappBot.Services;

namespace WhatsappBot.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CampaignApiController:ControllerBase
{
    private readonly CampgaignService _campgaignService;

    public CampaignApiController(CampgaignService campgaignService)
    {
        _campgaignService = campgaignService;
    }

    [HttpGet("GetAllCampaigns")]
    public async Task<IActionResult> GetAllCampaigns()
    {
        try
        {
            var campaigns = await _campgaignService.GetAllCampaigns();
            return Ok(campaigns);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("CreateCampaign")]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignDto request)
    {
        try
        {
            if (request.Name ==null || request.InitialMessage==null) return BadRequest("Name and initial message must not be null");

            var response = await _campgaignService.CreateCampaign(request);

            return Ok(new { response });
        }
        catch (Exception e)
        {
            return BadRequest(new{e.Message});
        }
    }

    [HttpDelete("{id}", Name = "DeleteCampaign")]
    public async Task<IActionResult> DeleteCampaign(Guid id)
    {
        try
        {
            if (id==Guid.Empty) return BadRequest("The id is null");
            var response = await _campgaignService.DeleteCampaign(id);
            if (response == null) return NotFound("Campaign with the given id not found");

            return Ok($"Campaign with id {id} deleted");
        }
        catch (Exception e)
        {
            return BadRequest(new{e.Message});
        }
    }
    // [HttpPut("{id}",Name = "UpdateCampaign")]
    // public 
    
}