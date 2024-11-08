using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatsappBot.Data;
using WhatsappBot.Models;
using WhatsappBot.Services;

namespace WhatsappBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScrapeController(WebScraperService scraperService, AppDbContext db) : ControllerBase
{
    [HttpGet("scrape")]
    public async Task<IActionResult> GetScrapedProducts()
    {
       
        try
        {
            await scraperService.FetchProductsAsync();
            return Ok("Products scraped and saved successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while scraping products: {ex.Message}");
        }
    }
    
    [HttpGet("get-all-products")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await db.Products.ToListAsync();
        return Ok(products);
    }

    [HttpGet("get-product")]
    public async Task<IList<Product>> GetProduct(string category)
    {
        var keywords = category.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        IQueryable<Product> query = db.Products;
        foreach (var keyword in keywords)
        {
            query = query.Where(p => EF.Functions.Like(p.Link, $"%{keyword}%"));
        }
        return await query.ToListAsync();
    }
}

