using System.Collections.Concurrent;

namespace WhatsappBot.Services;

public class OpenAiSessionManager
{
    private readonly ConcurrentDictionary<string, OpenAiRealtimeClient> _clients = new();
    private readonly TwilioMessageService _twilioClient;
    private readonly IServiceProvider _serviceProvider;

    public OpenAiSessionManager(TwilioMessageService twilioClient,
        IServiceProvider serviceProvider)
    {
        _twilioClient = twilioClient;
        _serviceProvider = serviceProvider;
    }

    public async Task<OpenAISessionResponse> GetOrCreateClientAsync(string sessionId, string msg)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var qdrantService = scope.ServiceProvider.GetRequiredService<QdrantService>();
            var client = _clients.GetValueOrDefault(sessionId);

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
                    var cl = new OpenAiRealtimeClient(sessionId, _twilioClient, qdrantService);
                    await cl.ConnectAsync(msg);
                    Console.WriteLine("Connected session: " + sessionId);
                    return cl;
                })),
                IsNewSession = true
            };
        }
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