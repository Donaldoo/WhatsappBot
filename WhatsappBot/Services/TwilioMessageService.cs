using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenAI.Chat;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using WhatsappBot.Data;
using WhatsappBot.Models;

namespace WhatsappBot.Services;

public class TwilioMessageService
{
    private readonly IConfiguration _configuration;
    private readonly ChatClient _chatClient;
    public TwilioMessageService(IConfiguration configuration)
    {
        _configuration = configuration;
        _chatClient = new ChatClient("gpt-4o",
            "sk-glWCbr31sohrUuM-ls8S1GCm8xD-njSu-p0sk5mneAT3BlbkFJEbdcsNmuL5YLIma1m-05zJdVDxwTHB1E2hM8tfyJsA");
    }

    public async Task<MessageResource> SendMessageAsync(string to, string botResponse)
    {
        TwilioClient.Init(_configuration["Twilio:AccountSid"], _configuration["Twilio:AuthToken"]);
        var messageOptions = new CreateMessageOptions(
            new PhoneNumber(to));
        messageOptions.From = new PhoneNumber(_configuration["Twilio:PhoneNumber"]);
        messageOptions.Body = botResponse;
    
        var message = await MessageResource.CreateAsync(messageOptions);
        return message;
    }
    
    public async Task<MessageResource> SendInitialMessageAsync(string to, string content)
    {
        TwilioClient.Init(_configuration["Twilio:AccountSid"], _configuration["Twilio:AuthToken"]);
        
        var contentVariables = new Dictionary<string, object>
        {
            {"1", content},
        };
        var serializedContentVariables = JsonConvert.SerializeObject(contentVariables);
        
        var messageOptions = new CreateMessageOptions(
            new PhoneNumber($"whatsapp:{to}"));
        messageOptions.From = new PhoneNumber(_configuration["Twilio:PhoneNumber"]);
        messageOptions.ContentSid = "HXafdf4456d75a932a717ef1384f906394";
        messageOptions.Body = null;
        messageOptions.ContentVariables = serializedContentVariables;
        
        
        var message = await MessageResource.CreateAsync(messageOptions);
        return message;
    }

    public async Task<ConversationMessage> GetBotSummary(string userNumber)
    {
        var allMessages = await GetMessagesFromUser(userNumber);
        var rating = 0;
        var jsonChatHistory = JsonConvert.SerializeObject(allMessages);
        const string prompt = "Respond as a human and in a friendly manner. Always respond in the language the user is using." +
                              $".Based on the conversation between you and the user make a very short summary .If you think the user is interested of buying the product or not." + 
                              "At the end of the response, include a line in this exact format: Rating: X, where X is a number from 1 to 5 representing the user's interest level."+
                              "The messages that are from whatsapp:+12706814859 are the messages that you sent to the user." +
                              "Other messages are the messages that the user sent to you.";

        var openAiResponse = await _chatClient.CompleteChatAsync(jsonChatHistory, prompt);
        var botResponse = openAiResponse.Value.Content[0].Text;

     
        var lines = botResponse.Split('\n');
        var summary = string.Join("\n", lines.Take(lines.Length - 1)).Trim(); 
        var ratingLine = lines.LastOrDefault()?.Trim(); 

    
        if (ratingLine != null && ratingLine.StartsWith("Rating:"))
        {
            if (int.TryParse(ratingLine.Replace("Rating:", "").Trim(), out var extractedRating))
            {
                rating = extractedRating;
            }
        }

        return new ConversationMessage
        {
            Summary = summary,
            Rating = rating
        };
    }
    public async Task<List<Conversation>> GetMessagesFromUser(string userNumber)
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
        var allMessages = incomingMessagess.Select(msg => new Conversation
            {
                Body = msg.Body,
                From = msg.From.ToString(),
                To = msg.To.ToString(),
                DateSent = msg.DateSent,
                IsBot = false,
               
            })
            .Concat(outgoingMessages.Select(msg => new Conversation
            {
                Body = msg.Body,
                From = msg.From.ToString(),
                To = msg.To.ToString(),
                DateSent = msg.DateSent,
                IsBot = true,
                
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