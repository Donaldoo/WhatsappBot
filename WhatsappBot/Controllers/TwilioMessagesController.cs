using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WhatsappBot.Services;

namespace WhatsappBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwilioMessagesController : ControllerBase
{
    private readonly OpenAiService _openAiService;
    private readonly TwilioMessageService _twilioMessageService;
    private readonly PhoneNumberService _phoneNumberService;
    public TwilioMessagesController(OpenAiService openAiService, TwilioMessageService twilioMessageService, PhoneNumberService phoneNumberService)
    {
        _openAiService = openAiService;
        _twilioMessageService = twilioMessageService;
        _phoneNumberService = phoneNumberService;
    }

    [HttpPost("send")]
    public async Task SendMessage([FromForm] IFormCollection message)
    {
        try
        {
            var response = await _openAiService.GenerateBotMessage(message["body"][0], message["from"][0]);
            await _twilioMessageService.SendMessageAsync(message["from"][0], response);
            await _phoneNumberService.CreatePhoneNumber(message["from"][0]);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpPost("send-template")]
    public async Task<IActionResult> SendTemplate([FromQuery] string to)
    {
        try
        {
            await _twilioMessageService.SendTemplateAsync(to);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("getLatestMessages")]
    public async Task<IActionResult> GetTheLastMessages(string to)
    {
        try
        {
            if (string.IsNullOrEmpty(to))
            {
                return BadRequest("User phone number is required.");
            }
            var response = await _twilioMessageService.GetMessagesFromUser(to);
            if (response == null || response.Count == 0)
            {
                return NotFound(new { error = "No messages found for the provided user." });
            }
            return Ok(response);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, new { error = "Internal server error", message = e.Message });
        }
    }
}