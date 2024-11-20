namespace WhatsappBot.Models;

public class ConversationMessage
{
    public string Body { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public DateTimeOffset? DateSent { get; set; }
}