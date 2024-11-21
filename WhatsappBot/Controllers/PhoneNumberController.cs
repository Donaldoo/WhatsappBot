using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> CreatePhoneNumber(string phoneInput)
    {
        var response = await _phoneNumberService.CreatePhoneNumber(phoneInput);
       return StatusCode(201, response);
    }
}