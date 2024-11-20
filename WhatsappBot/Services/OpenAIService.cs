using Newtonsoft.Json;
using OpenAI.Chat;

namespace WhatsappBot.Services;

public class OpenAiService
{
    private readonly ChatClient _chatClient;
    private readonly TwilioMessageService _twilioMessageService;

    public OpenAiService(TwilioMessageService twilioMessageService)
    {
        _twilioMessageService = twilioMessageService;
        _chatClient = new ChatClient("gpt-4o",
            "sk-glWCbr31sohrUuM-ls8S1GCm8xD-njSu-p0sk5mneAT3BlbkFJEbdcsNmuL5YLIma1m-05zJdVDxwTHB1E2hM8tfyJsA");
    }

    public async Task<string> GenerateBotMessage(string userInput, string phoneNumber)
    {
        var chatHistory = await _twilioMessageService.GetMessagesFromUser(phoneNumber);
        var jsonChatHistory = JsonConvert.SerializeObject(chatHistory);
        var prompt = "Respond as a human and in a friendly manner. Always respond in the language the user is using." +
                     $"These are your past messages with the user: {jsonChatHistory}. Use the history as context to respond to the user." +
                     "The messages that are from whatsapp:+12706814859 are the messages that you sent to the user." +
                     "Other messages are the messages that the user sent to you.";

        var openAiResponse = await _chatClient.CompleteChatAsync(userInput, prompt);
        return $"{openAiResponse.Value.Content[0].Text}";
    } 
}