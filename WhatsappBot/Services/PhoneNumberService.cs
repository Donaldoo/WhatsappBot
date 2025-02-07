using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Twilio.Types;
using WhatsappBot.Data;
using WhatsappBot.Models;
using WhatsappBot.Models.DTO;

namespace WhatsappBot.Services;

public class PhoneNumberService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    public PhoneNumberService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PhoneNumbersDto>> GetAllPhoneNumbers()
    {
        try
        {
            IEnumerable<PhoneNumbers> phoneNumberList = await _db.PhoneNumbers
                .Include(p => p.CampaignNumbersCollection)
                .ThenInclude(pc => pc.Campaign)
                .ToListAsync();
            var phoneNumbers = _mapper.Map<List<PhoneNumbersDto>>(phoneNumberList);
            return phoneNumbers;

        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    public async Task<PhoneNumbers> GetPhoneNumberById(Guid id)
    {
        try
        {
            var phoneNumber = await _db.PhoneNumbers.FirstOrDefaultAsync(u=>u.Id==id);
            return phoneNumber;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<PhoneNumbers> CreatePhoneNumber(string phoneInput)
    {
        var formattedPhoneNumber  = phoneInput.Replace("whatsapp:", "").Trim();
    
        var phoneNumber = new PhoneNumbers
        {
            PhoneNumber = formattedPhoneNumber
        };
        var existingPhoneNumber =
            await _db.PhoneNumbers.FirstOrDefaultAsync(n => n.PhoneNumber.ToLower() == formattedPhoneNumber);
        if (existingPhoneNumber!=null)
        {
            throw new InvalidOperationException("Number already exists");
        }
        await _db.PhoneNumbers.AddAsync(phoneNumber);
        await _db.SaveChangesAsync();
    
        return phoneNumber;
    }

    public async Task AddPhoneNumbersWithCampaign(IFormFile file,Guid campaignId)
    {
        try
        {
            if (file == null || file.Length == 0) throw new ArgumentException("Invalid file provided");

            var phoneNumbersToAdd = new List<PhoneNumbers>(); 
            using var reader = new StreamReader(file.OpenReadStream()); 
            var regex = new Regex(@"\+\d+");
        
            while (await reader.ReadLineAsync() is { } line)
            {
                var match = regex.Match(line);

                if (!match.Success) continue;
                var cleanedNumber = match.Value;
                
                phoneNumbersToAdd.Add(new PhoneNumbers { PhoneNumber = cleanedNumber,CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow});
            }

            await StorePhoneNumbersToDb(phoneNumbersToAdd);

            var phoneNumbers = await _db.PhoneNumbers.ToListAsync();
            
            await AddCampaignPhoneNumbers(phoneNumbers, campaignId);

        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while processing the file.", e);
        }
    }
    private async Task StorePhoneNumbersToDb(List<PhoneNumbers> phoneNumbersList)
    {
        var existingNumbers = await _db.PhoneNumbers
            .Where(p => phoneNumbersList.Select(n => n.PhoneNumber).Contains(p.PhoneNumber))
            .Select(p => p.PhoneNumber).ToListAsync();

        var newPhoneNumbers = phoneNumbersList.Where(p => !existingNumbers.Contains(p.PhoneNumber)).ToList();
        
        await _db.AddRangeAsync(newPhoneNumbers);
        await _db.SaveChangesAsync();
    }

    private async Task AddCampaignPhoneNumbers(IEnumerable<PhoneNumbers> phoneNumbersList,Guid campaignId)
    {
        var campaign = await _db.Campaigns
            .Include(c => c.CampaignNumbersCollection)
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign==null)
        {
            throw new KeyNotFoundException("Campaign not found");
        }

        var existingNumbersInCampaign = campaign.CampaignNumbersCollection.Select(n => n.PhoneNumberId);

        IEnumerable<Guid> numbersInCampaign = existingNumbersInCampaign.ToList();
        foreach (var phoneNumber in phoneNumbersList)
        {
            if (numbersInCampaign.Contains(phoneNumber.Id))
            {
                continue;
            }
            var campaignPhoneNumbers = new CampaignPhoneNumbers
            {
                CampaignId = campaign.Id,
                PhoneNumberId = phoneNumber.Id,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            campaign.CampaignNumbersCollection.Add(campaignPhoneNumbers);
        }
   
        await _db.SaveChangesAsync();
    }
    
}