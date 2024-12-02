using Microsoft.EntityFrameworkCore;
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

    public async Task CreatePhoneNumber(string phoneInput)
    {
        var formattedPhoneNumber  = phoneInput.Replace("whatsapp:", "").Trim();
        
        var existingPhoneNumber = await _db.PhoneNumbers.FirstOrDefaultAsync(u=>u.PhoneNumber==formattedPhoneNumber);
        if (existingPhoneNumber !=null)
        {
            await _db.PhoneNumbers.AddAsync(new PhoneNumbers
            {
                PhoneNumber = formattedPhoneNumber
            });
            await _db.SaveChangesAsync();     
        } 
       
    }
}