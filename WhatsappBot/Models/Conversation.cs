namespace WhatsappBot.Models;

public class Conversation
{
    public string Body { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public bool IsBot { get; set; }
   
    public DateTimeOffset? DateSent { get; set; }
}