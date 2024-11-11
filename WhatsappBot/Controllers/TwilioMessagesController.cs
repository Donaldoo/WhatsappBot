using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;
using WhatsappBot.Services;

namespace WhatsappBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwilioMessagesController(TwilioMessageService twilioMessageService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<MessageResource> SendMessage([FromForm] IFormCollection message)
    {
        try
        {
            return await twilioMessageService.SendMessageAsync(message["From"][0]);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}