#nullable enable
namespace WhatsappBot.Models;

public class PhoneNumbers
{
    public Guid Id{ get; set; } 
    public string PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }
    public ICollection<CampaignPhoneNumbers> CampaignNumbersCollection { get; set; } = new List<CampaignPhoneNumbers>();
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}