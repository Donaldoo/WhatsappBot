using System.Collections.Concurrent;
using WhatsappBot.Services;

public class OpenAiSessionManager
{
    private readonly ConcurrentDictionary<string, OpenAiRealtimeClient> _clients = new();
    private readonly TwilioMessageService _twilioClient;
    private readonly IConfiguration _configuration;

    public OpenAiSessionManager(TwilioMessageService twilioClient, IConfiguration configuration)
    {
        _twilioClient = twilioClient;
        _configuration = configuration;
    }

    public async Task<OpenAISessionResponse> GetOrCreateClientAsync(string sessionId, string msg)
    {
        var client = _clients.TryGetValue(sessionId, out var existingClient) ? existingClient : null;

        if (client != null)
        {
            return new OpenAISessionResponse
            {
                IsNewSession = false,
                Client = client
            };
        }


        return new OpenAISessionResponse()
        {
            Client = _clients.GetOrAdd(sessionId, await Task.Run(async () =>
            {
                var cl = new OpenAiRealtimeClient(sessionId, _twilioClient, _configuration);
                await cl.ConnectAsync(msg);
                return cl;
            })),
            IsNewSession = true
        };
}

    public async Task CloseSessionAsync(string sessionId)
    {
        if (_clients.TryRemove(sessionId, out var client))
        {
            await client.CloseAsync();
        }
    }
}

public record OpenAISessionResponse
{
    public bool IsNewSession { get; init; }
    public OpenAiRealtimeClient Client { get; init; }
}