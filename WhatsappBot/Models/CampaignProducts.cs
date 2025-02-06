namespace WhatsappBot.Models;

public class CampaignProducts
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    
    public Guid  CampaignId { get; set; }
    public Campaign Campaign { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}