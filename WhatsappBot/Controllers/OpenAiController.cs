using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI.Chat;
using WhatsappBot.Data;
using WhatsappBot.Services;

namespace WhatsappBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpenAiController : ControllerBase
{
    private readonly ChatClient _chatClient;
    private readonly AppDbContext _db;
    private readonly OpenAiRealtimeClient _client;
    private readonly QdrantService _qdrantService;

    public OpenAiController(AppDbContext db, OpenAiRealtimeClient client, QdrantService qdrantService)
    {
        _db = db;
        _client = client;
        _qdrantService = qdrantService;
        _chatClient = new ChatClient("gpt-4o",
            "sk-glWCbr31sohrUuM-ls8S1GCm8xD-njSu-p0sk5mneAT3BlbkFJEbdcsNmuL5YLIma1m-05zJdVDxwTHB1E2hM8tfyJsA");
    }

    [HttpPost("OpenAiInfo")]
    public async Task<IActionResult> Get(string userInput)
    {
        var allProducts = await _qdrantService.SearchProducts(userInput);
        var j = JsonConvert.SerializeObject(allProducts);
        var prompt =
            $"Je nje doktor dhe je duke folur me nje pacient. Ketu ke disa te dhena qe mund te ndihmojne {j}, bazuar ne keto te dhena zgjidh rreth 3 ilacet me te pershtatshme (emrat dhe cmimet) qe mund te ndihmojne kete pacient: '{userInput}'." +
            $"Pergjigjet ktheji si nje person real.";
        
        var openAiResponse = await _chatClient.CompleteChatAsync(prompt);
        return Ok($"{openAiResponse.Value.Content[0].Text}");
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] string userMessage)
    {
        try
        {
            var message = new
            {
                type = "conversation.item.create",
                item = new
                {
                    type = "message",
                    role = "user",
                    content = new[]
                    {
                        new
                        {
                            type = "input_text",
                            text = userMessage
                        }
                    }
                }
            };


            await _client.SendMessageAsync(userMessage);
            return Ok("Message sent.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpPost("close")]
    public async Task<IActionResult> CloseWebSocketCommunication()
    {
        try
        {
            await _client.CloseAsync();
            return Ok("WebSocket connection closed.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}