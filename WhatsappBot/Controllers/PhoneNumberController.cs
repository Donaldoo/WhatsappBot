using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WhatsappBot.Models;
using WhatsappBot.Services;

namespace WhatsappBot.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PhoneNumberController:ControllerBase
{
    private readonly PhoneNumberService _phoneNumberService;

    public PhoneNumberController(PhoneNumberService phoneNumberService)
    {
        _phoneNumberService = phoneNumberService;
    }

    [HttpGet("get-phone-numbers")]
    public async Task<IActionResult> GetPhoneNumbers()
    {
        var response = await _phoneNumberService.GetAllPhoneNumbers();
        return Ok(response);
    }
    
    [HttpGet("Get-PhoneNumber-ById")]
    public async Task<IActionResult> GetPhoneNumberById(Guid id)
    {
        var response = await _phoneNumberService.GetPhoneNumberById(id);
        if (response==null)
        {
            return NotFound();
        }
        return Ok(response);
    }

    [HttpPost("Create-phoneNumber")]
    public async Task<IActionResult> CreatePhoneNumber(IFormFile file)
    {
        try
        { 
            if (file == null || file.Length == 0) return BadRequest(new { text = "Invalid file uploaded" });
            
            await _phoneNumberService.AddPhoneNumbers(file);
            return StatusCode(201, new { message = "Numbers added to database" });

        }
        catch (Exception e)
        {
            return Ok(new
            {
                text = e.InnerException is PostgresException { SqlState: "23505" }
                    ? "The numbers are already added in database"
                    : "There was an error processing you request"
            });
        }
    }
}