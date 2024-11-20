using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using WhatsappBot.Models;

namespace WhatsappBot.Services;

public class TwilioMessageService
{
    private readonly IConfiguration _configuration;

    public TwilioMessageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<MessageResource> SendMessageAsync(string from, string botResponse)
    {
        TwilioClient.Init(_configuration["Twilio:AccountSid"], _configuration["Twilio:AuthToken"]);
        var messageOptions = new CreateMessageOptions(
            new PhoneNumber(from));
        messageOptions.From = new PhoneNumber(_configuration["Twilio:PhoneNumber"]);
        messageOptions.Body = botResponse;

        var message = await MessageResource.CreateAsync(messageOptions);
        return message;
    }

    public async Task<List<ConversationMessage>> GetMessagesFromUser(string userNumber)
    {
        if (string.IsNullOrEmpty(userNumber))
        {
            throw new ArgumentException("The 'userNumber' cannot be null or empty.");
        }


        TwilioClient.Init(_configuration["Twilio:AccountSid"], _configuration["Twilio:AuthToken"]);

        var incomingMessagess = await MessageResource.ReadAsync(
            to: new PhoneNumber(_configuration["Twilio:PhoneNumber"]),
            from: new PhoneNumber(userNumber),
            limit: 10
        );
        var outgoingMessages = await MessageResource.ReadAsync(
            from: new PhoneNumber(_configuration["Twilio:PhoneNumber"]),
            to: new PhoneNumber(userNumber),
            limit: 10
        );
        var allMessages = incomingMessagess.Select(msg => new ConversationMessage
            {
                Body = msg.Body,
                From = msg.From.ToString(),
                To = msg.To.ToString(),
                DateSent = msg.DateSent
            })
            .Concat(outgoingMessages.Select(msg => new ConversationMessage
            {
                Body = msg.Body,
                From = msg.From.ToString(),
                To = msg.To.ToString(),
                DateSent = msg.DateSent
            }))
            .OrderBy(msg => msg.DateSent)
            .ToList();

        return allMessages;
    }

    public async Task SendTemplateAsync(string to)
    {
        TwilioClient.Init(_configuration["Twilio:AccountSid"], _configuration["Twilio:AuthToken"]);
        var messageOptions = new CreateMessageOptions(
            new PhoneNumber(to));
        messageOptions.From = new PhoneNumber(_configuration["Twilio:PhoneNumber"]);
        messageOptions.ContentSid = "HXc70a08e32847469cea1888618f5c29db";
        var message = await MessageResource.CreateAsync(messageOptions);
    }
}