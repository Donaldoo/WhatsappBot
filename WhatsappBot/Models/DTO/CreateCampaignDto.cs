using System.ComponentModel.DataAnnotations;

namespace WhatsappBot.Models.DTO;

public class CreateCampaignDto
{
    [Required]
    public string Name { get; set; }

    public CampaignStatus Status { get; set; } = CampaignStatus.Pending;
    public string InitialMessage { get; set; }
}