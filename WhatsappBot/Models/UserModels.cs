using Newtonsoft.Json;

namespace WhatsappBot.Models;

public class MessagePayload
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("item")]
    public ItemPayload Item { get; set; }
}

public class ItemPayload
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("content")]
    public List<ContentPayload> Content { get; set; }
}

public class ContentPayload
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
}