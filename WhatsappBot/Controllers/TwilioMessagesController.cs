using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WhatsappBot.Services;

namespace WhatsappBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwilioMessagesController : ControllerBase
{
    private readonly OpenAiSessionManager _openAiSessionManager;

    public TwilioMessagesController(OpenAiSessionManager openAiSessionManager)
    {
        _openAiSessionManager = openAiSessionManager;
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
}