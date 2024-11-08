using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenAI.Chat;
using WhatsappBot.Data;

namespace WhatsappBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpenAiController : ControllerBase
{
    private readonly ChatClient _chatClient;
    private readonly AppDbContext _db;

    public OpenAiController(AppDbContext db)
    {
        _db = db;
        _chatClient = new ChatClient("gpt-3.5-turbo",
            "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");
    }

    [HttpPost("OpenAiInfo")]
    public async Task<IActionResult> Get(string userInput)
    {
        var prompt = $"Extract relevant keywords for searching products from the request: '{userInput}'";
       var openAiResponse = await _chatClient.CompleteChatAsync(prompt);

       var keywords = openAiResponse.Value.Content[0].Text.Replace(" ", " & ");
       var query = @"SELECT * FROM ""Products"" WHERE to_tsvector('simple', ""Name"" || ' ' || ""Description"" || ' ' || regexp_replace(""Link"", '[-/]', ' ', 'g')) @@ to_tsquery(@keywords)";
        var products = await _db.Products
            .FromSqlRaw(query,
                new NpgsqlParameter("keywords", NpgsqlTypes.NpgsqlDbType.Text) { Value = keywords })
            .ToListAsync();
        
        if (products.Count == 0)
        {
            return Ok($"No products found for '{userInput}'.");
        }

        var responsePrompt = $"The available products for '{userInput}' include:\n";
        responsePrompt = products.Aggregate(responsePrompt, (current, product) => current + $"- {product.Name}: {product.Description}, priced at {product.PriceNotFormatted}\n");
        
        return Ok($"{responsePrompt}");
    }
}