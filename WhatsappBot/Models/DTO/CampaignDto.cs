namespace WhatsappBot.Models.DTO;

public class CampaignDto
{
    public Guid Id { get; set; }
   
    public string Name { get; set; }

    public CampaignStatus Status { get; set; } = CampaignStatus.Pending;
    public string InitialMessage { get; set; }
  
}