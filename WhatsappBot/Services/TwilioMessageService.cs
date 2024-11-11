using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace WhatsappBot.Services;

public class TwilioMessageService
{
    private readonly IConfiguration _configuration;
    public TwilioMessageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<MessageResource> SendMessageAsync(string from)
    {
        TwilioClient.Init(_configuration["Twilio:AccountSid"], _configuration["Twilio:AuthToken"]);
        var messageOptions = new CreateMessageOptions(
            new PhoneNumber(from));
        messageOptions.From = new PhoneNumber(_configuration["Twilio:PhoneNumber"]);
        messageOptions.Body = "Your appointment is coming up on July 21 at 3PM";
        messageOptions.MediaUrl = new List<Uri>()
        {
            new Uri("https://t4.ftcdn.net/jpg/00/68/80/45/360_F_68804542_8okyINK2f0DtBlGBQBeNrp2sIh5yvQnt.jpg")
        };

        var message = await MessageResource.CreateAsync(messageOptions);
        return message;
    }
}