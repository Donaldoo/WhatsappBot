namespace WhatsappBot.Models.DTO;

public class PhoneNumbersDto
{
    public Guid Id{ get; set; } 
    public string PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }
    public ICollection<CampaignDto> Campaigns { get; set; }
}