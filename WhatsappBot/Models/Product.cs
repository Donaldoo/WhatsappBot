namespace WhatsappBot.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Number { get; set; }
    public string PriceNotFormatted { get; set; }
    public int StockValue { get; set; }
    public string Link { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public ICollection<CampaignProducts> CampaignProductsCollectionCollection { get; set; } = new List<CampaignProducts>();
}

public class ProductsContainer
{
    public IList<Product> Product { get; set; }
}

public class Root
{
    public IList<ProductsContainer> ProductsContainer { get; set; }
}