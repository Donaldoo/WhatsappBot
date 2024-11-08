using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using WhatsappBot.Data;
using WhatsappBot.Models;

namespace WhatsappBot.Services;

public class WebScraperService(AppDbContext db, IHttpClientFactory httpClientFactory)
{
    public async Task FetchProductsAsync()
    {
        const string url = "https://www.ditenate.al/produktet?pagesize=5000&feed=true&DoNotShowVariantsAsSingleProducts=True";
        var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5);

        var response = await httpClient.GetStringAsync(url);

        using var document = JsonDocument.Parse(response);

        var cleanedResponse = response.Trim();
        var productData = JsonSerializer.Deserialize<List<Root>>(cleanedResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var rootData = productData[0];

        foreach (var container in rootData.ProductsContainer)
        {
            if (container.Product == null) continue;
            foreach (var product in container.Product)
            {
                product.Description = CleanDescription(product.Description);
                product.Image = FixImageUrl(product.Image);
                product.Link = FormatLink(product.Link);

                var existingProduct = await db.Products
                    .FirstOrDefaultAsync(p => p.Number == product.Number);

                if (existingProduct == null)
                {
                    db.Products.Add(product);
                    await db.SaveChangesAsync();
                }
                else
                { 
                    existingProduct.Link = product.Link;
                    existingProduct.Description = product.Description;
                    existingProduct.PriceNotFormatted = product.PriceNotFormatted;
                    db.Products.Update(existingProduct);
                    await db.SaveChangesAsync();
                }
            }
        }
    }

    private static string FixImageUrl(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return imageUrl;

        var decodedUrl = Uri.UnescapeDataString(imageUrl);

        if (!decodedUrl.StartsWith("https://www.ditenate.al"))
        {
            decodedUrl = "https://www.ditenate.al" + decodedUrl;
        }

        return decodedUrl;
    }

    private static string CleanDescription(string htmlDescription)
    {
        var decodedDescription = WebUtility.HtmlDecode(htmlDescription);
        var cleanDescription = Regex.Replace(decodedDescription, "<.*?>", string.Empty);
        return cleanDescription.Trim();
    }
    
    private static string FormatLink(string link)
    {
        if (string.IsNullOrEmpty(link)) return link;
        link = link.TrimStart('/');
        link = link.Replace("/", ", ");
        link = link.Replace("-", " ");

        return link;
    }
}