using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Twilio;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WhatsappBot.Services;

public class OpenAiRealtimeClient
{
    private readonly Uri _uri;
    private readonly ClientWebSocket _socket;
    private readonly ConcurrentQueue<string> _receivedMessages = new(); // Queue to store received messages
    private readonly TwilioMessageService _twilioClient;
    private readonly IConfiguration _configuration;
    private readonly string _sessionId;

    public OpenAiRealtimeClient(string sessionId, TwilioMessageService twilioClient, IConfiguration configuration)
    {
        _sessionId = sessionId;
        _twilioClient = twilioClient;
        _configuration = configuration;
        var apiKey = "sk-glWCbr31sohrUuM-ls8S1GCm8xD-njSu-p0sk5mneAT3BlbkFJEbdcsNmuL5YLIma1m-05zJdVDxwTHB1E2hM8tfyJsA";
        _uri = new Uri("wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01");
        _socket = new ClientWebSocket();
        _socket.Options.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        _socket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
    }

    public async Task ConnectAsync(string initMsg)
    {
        if (_socket.State == WebSocketState.Open)
        {
            await CloseAsync();
        }
        
        var cancellationToken = new CancellationToken();

        await _socket.ConnectAsync(_uri, cancellationToken);
        Console.WriteLine("Connected to OpenAI Realtime API.");

        var initialMessage = new
        {
            type = "response.create",
            response = new
            {
                modalities = new[] { "text" },
                instructions = "Please respond like a friendly, helpful human. Your answers should be in plain text only, with no JSON or structured formats. Keep your responses conversational, natural, and clear, offering helpful advice or answers. Be empathetic and kind in your tone, tailoring your responses based on the user’s message. If the user asks for clarification, provide clear and concise details. Avoid jargon unless the user specifically asks for it. Always aim to be helpful, polite, and approachable in all interactions.",
            }
        };

        await SendMessageAsync(initialMessage);
        _ = Task.Run(async () => await ReceiveMessagesAsync(), cancellationToken);
    }

    public async Task SendMessageAsync(object messageObject)
    {
        var responseCreateMessage = new
        {
            type = "response.create",
            response = new
            {
                modalities = new[] { "text" },
            }
        };

        var conversationItemMessageJson = JsonSerializer.Serialize(messageObject);
        var conversationItemBytes = Encoding.UTF8.GetBytes(conversationItemMessageJson);
        Console.WriteLine("Sending message: " + conversationItemMessageJson);
        await _socket.SendAsync(new ArraySegment<byte>(conversationItemBytes), WebSocketMessageType.Text, true,
            CancellationToken.None);

        var responseCreateMessageJson = JsonSerializer.Serialize(responseCreateMessage);
        var responseCreateBytes = Encoding.UTF8.GetBytes(responseCreateMessageJson);
        await _socket.SendAsync(new ArraySegment<byte>(responseCreateBytes), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4];
        var messageBuilder = new StringBuilder();
        var cancellationToken = new CancellationToken();

        while (_socket.State == WebSocketState.Open)
        {
            try
            {
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseAsync();
                    break;
                }

                var messagePart = Encoding.UTF8.GetString(buffer, 0, result.Count);
                messageBuilder.Append(messagePart);

                if (result.EndOfMessage)
                {
                    var completeMessage = messageBuilder.ToString();
                    messageBuilder.Clear();

                    using (var jsonDocument = JsonDocument.Parse(completeMessage))
                    {
                        var root = jsonDocument.RootElement;

                        if (root.TryGetProperty("text", out JsonElement textElement))
                        {
                            Console.WriteLine("---------------------_" + completeMessage);
                            Console.WriteLine("Received message: " + textElement.GetString());
                            await _twilioClient.SendMessageAsync(_sessionId, textElement.GetString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving message: " + ex.Message);
                break;
            }
        }
    }

    public async Task CloseAsync()
    {
        var cancellationToken = new CancellationToken();
        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
        Console.WriteLine("Connection closed.");
    }
}