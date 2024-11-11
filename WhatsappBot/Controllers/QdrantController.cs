using Microsoft.AspNetCore.Mvc;
using WhatsappBot.Models;
using WhatsappBot.Services;

namespace WhatsappBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QdrantController : ControllerBase
{
    private readonly QdrantService _qdrantService;

    public QdrantController(QdrantService qdrantService)
    {
        _qdrantService = qdrantService;
    }

    [HttpGet("get-collections")]
    public async Task<IActionResult> GetCollections()
    {
        var collections = await _qdrantService.GetCollections();
        return Ok(collections);
    }
    
    [HttpPost("create-collection")]
    public async Task<IActionResult> CreateCollection([FromBody] string collectionName)
    {
        await _qdrantService.CreateCollection(collectionName);
        return Ok();
    }
    
    [HttpPost("insert-embedding")]
    public async Task<IActionResult> InsertEmbedding(Product product)
    {
        await _qdrantService.InsertDocument(new Product
        {
            Number = product.Number,
            Name = product.Name,
            Description = product.Description,
            Link = product.Link,
            PriceNotFormatted = product.PriceNotFormatted
        });
        return Ok();
    }
    
    [HttpPost("insert-all")]
    public async Task<IActionResult> InsertAll()
    {
        await _qdrantService.InsertAllProducts();
        return Ok();
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        var products = await _qdrantService.SearchProducts(query);
        return Ok(products);
    }
}