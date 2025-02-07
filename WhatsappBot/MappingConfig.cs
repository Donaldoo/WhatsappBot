using AutoMapper;
using WhatsappBot.Models;
using WhatsappBot.Models.DTO;

namespace WhatsappBot;

public class MappingConfig:Profile
{
    public MappingConfig()
    {
        CreateMap<PhoneNumbers, PhoneNumbersDto>()
            .ForMember(dest => dest.Campaigns,
                opt => opt.MapFrom(src =>
                    src.CampaignNumbersCollection.Select(pc => new CampaignDto
                        { Id = pc.CampaignId, Name = pc.Campaign.Name ,InitialMessage = pc.Campaign.InitialMessage,Status = pc.Campaign.Status})));


        CreateMap<Campaign, CampaignDto>();
        CreateMap<CampaignDto, Campaign>();

    }
}