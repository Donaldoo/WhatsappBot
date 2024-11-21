#nullable enable
using System.ComponentModel.DataAnnotations;

namespace WhatsappBot.Models;

public class PhoneNumbers
{
    public Guid Id{ get; set; }
    public string PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }
}