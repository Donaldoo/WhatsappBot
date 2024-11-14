
using Microsoft.EntityFrameworkCore;
using OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using WhatsappBot.Data;
using WhatsappBot.Models;

namespace WhatsappBot.Services;

public class QdrantService
{ 
    private readonly QdrantClient _client;
    private readonly OpenAIClient _openAiClient;
    private readonly AppDbContext _db;

    public QdrantService(IConfiguration configuration, AppDbContext db)
    {
        _db = db;
        _openAiClient = new OpenAIClient("sk-glWCbr31sohrUuM-ls8S1GCm8xD-njSu-p0sk5mneAT3BlbkFJEbdcsNmuL5YLIma1m-05zJdVDxwTHB1E2hM8tfyJsA");
        _client = new QdrantClient(host:configuration["Qdrant:ClusterUrl"], https:true, apiKey:configuration["Qdrant:ApiKey"]);
    }
    
    public async Task<IReadOnlyList<string>> GetCollections()
    {
        return await _client.ListCollectionsAsync();
    }
    
    public async Task CreateCollection(string collectionName)
    {
        var vectorParams = new VectorParams
        {
            Size = 1536,
            Distance = Distance.Cosine
        };
        await _client.CreateCollectionAsync(collectionName, vectorParams);
    }
    
    public async Task<float[]> GenerateEmbedding(string input)
    {
        var embeddingService = _openAiClient.GetEmbeddingClient("text-embedding-3-small");
        var response = await embeddingService.GenerateEmbeddingAsync(input);
        return response.Value.ToFloats().ToArray();
    }
    
    public async Task InsertDocument(Product product)
    {
        var embedding = await GenerateEmbedding($"{product.Name} {product.Description} {product.Link}");
        
        if (!ulong.TryParse(product.Number, out var pointId))
        {
            throw new ArgumentException("Product number must be a valid unsigned long integer.");
        }
        
        await _client.UpsertAsync("products_collection", new List<PointStruct>
        {
            new PointStruct
            {
                Id = pointId,
                Vectors = embedding,
                Payload =
                {
                    ["name"] = product.Name,
                    ["price"] = product.PriceNotFormatted,
                    ["link"] = product.Link,
                    ["description"] = product.Description
                }
            }
        });
    }

    public async Task InsertAllProducts()
    {
        var allProducts = await _db.Products.ToListAsync();
        foreach (var product in allProducts)
        {
            await InsertDocument(product);
        }
    }
    
    public async Task<IReadOnlyList<Product>> SearchProducts(string userInput)
    {
        var embedding = await GenerateEmbedding(userInput);
        var searchResults = await _client.SearchAsync("products_collection", embedding, limit:10);
        var products = searchResults.Select(result => new Product
        {
            Number = result.Id.Num.ToString(),
            Name = result.Payload.TryGetValue("name", out var name) ? name.ToString() : "Unknown",
            Description = result.Payload.TryGetValue("description", out var description) ? description.ToString() : "No description",
            Link = result.Payload.TryGetValue("link", out var link) ? link.ToString() : "No link",
            PriceNotFormatted = result.Payload.TryGetValue("price", out var price) ? price.ToString() : "0"
        }).ToList();

        return products;
    }
}