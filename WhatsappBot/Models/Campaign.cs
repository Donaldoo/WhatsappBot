using System.ComponentModel.DataAnnotations;

namespace WhatsappBot.Models;

public enum CampaignStatus
{
    Pending,
    Sent
}
public class Campaign
{
    public Guid Id { get; set; }
   
    public string Name { get; set; }

    public CampaignStatus Status { get; set; } = CampaignStatus.Pending;
    public string InitialMessage { get; set; }
    public ICollection<CampaignPhoneNumbers> CampaignNumbersCollection { get; set; } = new List<CampaignPhoneNumbers>();
    public ICollection<CampaignProducts> CampaignProductsCollectionCollection { get; set; } = new List<CampaignProducts>();
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
