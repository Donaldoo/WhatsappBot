namespace WhatsappBot.Models;

public class CampaignPhoneNumbers
{
    public Guid PhoneNumberId { get; set; }
    public PhoneNumbers PhoneNumber { get; set; }
    
    public Guid  CampaignId { get; set; }
    public Campaign Campaign { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}