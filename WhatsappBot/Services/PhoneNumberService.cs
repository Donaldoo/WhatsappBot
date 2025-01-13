using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Twilio.Types;
using WhatsappBot.Data;
using WhatsappBot.Models;

namespace WhatsappBot.Services;

public class PhoneNumberService
{
    private readonly AppDbContext _db;

    public PhoneNumberService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<PhoneNumbers>> GetAllPhoneNumbers()
    {
        try
        {
            IEnumerable<PhoneNumbers> phoneNumberList = await _db.PhoneNumbers.ToListAsync();
            
            return phoneNumberList;

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
        string formattedPhoneNumber  = phoneInput.Replace("whatsapp:", "").Trim();
    
        var phoneNumber = new PhoneNumbers
        {
            PhoneNumber = formattedPhoneNumber
        };
       await _db.AddAsync(phoneNumber);
       await _db.SaveChangesAsync();
    
        return phoneNumber;
    }

  
    public async Task AddPhoneNumbers(IFormFile file)
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
                
            phoneNumbersToAdd.Add(new PhoneNumbers { PhoneNumber = cleanedNumber });
        }
        const int batchSize = 1000;
        foreach (object[] batch in phoneNumbersToAdd.Chunk(batchSize))
        {
            await _db.AddRangeAsync(batch);
            await _db.SaveChangesAsync();
        }
    }
}