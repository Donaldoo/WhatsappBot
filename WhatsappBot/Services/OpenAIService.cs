using Newtonsoft.Json;
using OpenAI.Chat;

namespace WhatsappBot.Services;

public class OpenAiService
{
    private readonly QdrantService _qdrantService;
    private readonly ChatClient _chatClient;

    public OpenAiService(QdrantService qdrantService)
    {
        _qdrantService = qdrantService;
        _chatClient = new ChatClient("gpt-4o",
            "sk-glWCbr31sohrUuM-ls8S1GCm8xD-njSu-p0sk5mneAT3BlbkFJEbdcsNmuL5YLIma1m-05zJdVDxwTHB1E2hM8tfyJsA");
    }

    public async Task<string> GenerateBotMessage(string userInput)
    {
        var allProducts = await _qdrantService.SearchProducts(userInput);
        var j = JsonConvert.SerializeObject(allProducts);
        var prompt =
            $"Je nje doktor dhe je duke folur me nje pacient. Ketu ke disa te dhena qe mund te ndihmojne {j}, bazuar ne keto te dhena zgjidh rreth 3 ilacet me te pershtatshme (emrat dhe cmimet) qe mund te ndihmojne kete pacient: '{userInput}'." +
            $"Pergjigjet ktheji si nje person real.";
        
        var openAiResponse = await _chatClient.CompleteChatAsync(prompt);
        return $"{openAiResponse.Value.Content[0].Text}";
    } 
}