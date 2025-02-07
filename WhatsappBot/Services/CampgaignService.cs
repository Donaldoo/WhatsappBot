using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WhatsappBot.Data;
using WhatsappBot.Models;
using WhatsappBot.Models.DTO;

namespace WhatsappBot.Services;

public class CampgaignService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public CampgaignService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }
     
    public async Task<IEnumerable<CampaignDto>> GetAllCampaigns()
    {
        try
        {
            IEnumerable<Campaign> campaignList = await _db.Campaigns.ToListAsync();

            var campaigns = _mapper.Map<List<CampaignDto>>(campaignList);
            return campaigns; 
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<Guid> CreateCampaign(CreateCampaignDto createCampaign)
    {
        var existingCampaign = await _db.Campaigns.FirstOrDefaultAsync(c=>c.Name.ToLower()==createCampaign.Name.ToLower());
        if (existingCampaign!=null)
        {
            throw new InvalidOperationException("A campaign with the same name already exists.");
        }
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Name = createCampaign.Name,
            InitialMessage = createCampaign.InitialMessage,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        await _db.Campaigns.AddAsync(campaign);
        await _db.SaveChangesAsync();

        return campaign.Id;
    }

    public async Task<Campaign> DeleteCampaign(Guid id)
    {
        var campaign =await _db.Campaigns.FirstOrDefaultAsync(p => p.Id == id);
         _db.Campaigns.Remove(campaign);
         await _db.SaveChangesAsync();

         return campaign;
    }

    public async Task UpdateCampaign(UpdateCampaignDto campaignDto)
    {
        var campaign = _mapper.Map<Campaign>(campaignDto); 
        _db.Campaigns.Update(campaign);
      await _db.SaveChangesAsync();
    }
}
