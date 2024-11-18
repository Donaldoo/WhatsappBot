using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WhatsappBot.Services;

namespace WhatsappBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwilioMessagesController : ControllerBase
{
    private readonly OpenAiSessionManager _openAiSessionManager;
    private readonly TwilioMessageService _twilioMessageService;

    public TwilioMessagesController(OpenAiSessionManager openAiSessionManager, TwilioMessageService twilioMessageService)
    {
        _openAiSessionManager = openAiSessionManager;
        _twilioMessageService = twilioMessageService;
    }

    [HttpPost("send")]
    public async Task SendMessage([FromForm] IFormCollection message)
    {
        try
        {
            // var messageOptions = new
            // {
            //     type = "conversation.item.create",
            //     item = new
            //     {
            //         type = "message",
            //         role = "user",
            //         content = new[]
            //         {
            //             new
            //             {
            //                 type = "input_text",
            //                 text = message["Body"][0] + productsJson
            //             }
            //         }
            //     }
            // };
            var response = await _openAiSessionManager.GetOrCreateClientAsync(message["From"][0], message["body"][0]);
            if (!response.IsNewSession)
            {
                await response.Client.SendMessageAsync(message["Body"][0]);
            }
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