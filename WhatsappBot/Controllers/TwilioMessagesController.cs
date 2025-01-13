using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Npgsql;
using WhatsappBot.Data;
using WhatsappBot.Models;
using WhatsappBot.Services;

namespace WhatsappBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwilioMessagesController : ControllerBase
{
    private readonly OpenAiService _openAiService;
    private readonly TwilioMessageService _twilioMessageService;
    private readonly PhoneNumberService _phoneNumberService;
    private readonly AppDbContext _db;
    public TwilioMessagesController(OpenAiService openAiService, TwilioMessageService twilioMessageService, PhoneNumberService phoneNumberService, AppDbContext db)
    {
        _openAiService = openAiService;
        _twilioMessageService = twilioMessageService;
        _phoneNumberService = phoneNumberService;
        _db = db;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromForm] IFormCollection message)
    {
        try
        {
            var response = await _openAiService.GenerateBotMessage(message["body"][0], message["from"][0]);
            await _twilioMessageService.SendMessageAsync(message["from"][0], response);
            await _phoneNumberService.CreatePhoneNumber(message["from"][0]);
            return Ok();
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
   
    [HttpPost("send-initial-message")]
    public async Task<IActionResult> SendInitialMessage([FromBody] string message)
    {
        if (message==null)
        {
            return BadRequest("Message should not be null");
        }
    
        var failedNumbers = new List<string>();
        var phoneNumbers = await _phoneNumberService.GetAllPhoneNumbers();

        var distinctPhoneNumbers = phoneNumbers.DistinctBy(p => p.PhoneNumber).ToList();
        foreach (var phoneNumber in distinctPhoneNumbers)
        {
            try
            {
                await _twilioMessageService.SendInitialMessageAsync(phoneNumber.PhoneNumber, message);
            }
            catch (Exception e)
            {
                failedNumbers.Add(phoneNumber.PhoneNumber);
            }
        }
        return Ok(new
        {
            message = "Messages sent.",
            failedCount = failedNumbers.Count,
            failedNumbers
        });
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
            var response = await _twilioMessageService.GetMessagesFromUser($"whatsapp:"+to);
            if (response == null )
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
    [HttpGet("getBotSummary")]
    public async Task<IActionResult> GetBotSummary(string to)
    {
        try
        {
            if (string.IsNullOrEmpty(to))
            {
                return BadRequest("User phone number is required.");
            }
            var response = await _twilioMessageService.GetBotSummary($"whatsapp:"+to);
            if (response.Summary == null )
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