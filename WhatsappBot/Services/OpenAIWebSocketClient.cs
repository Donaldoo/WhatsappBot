using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using Twilio;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WhatsappBot.Services;

public class OpenAiRealtimeClient
{
    private readonly Uri _uri;
    private ClientWebSocket _socket;
    private readonly TwilioMessageService _twilioClient;
    private readonly string _sessionId;
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly QdrantService _qdrantService;

    public OpenAiRealtimeClient(string sessionId, TwilioMessageService twilioClient, QdrantService qdrantService)
    {
        _sessionId = sessionId;
        _twilioClient = twilioClient;
        _qdrantService = qdrantService;
        var apiKey = "sk-glWCbr31sohrUuM-ls8S1GCm8xD-njSu-p0sk5mneAT3BlbkFJEbdcsNmuL5YLIma1m-05zJdVDxwTHB1E2hM8tfyJsA";
        _uri = new Uri("wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01");
        InitializeWebSocket(apiKey);
    }

    private void InitializeWebSocket(string apiKey)
    {
        _socket = new ClientWebSocket();
        _socket.Options.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        _socket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
    }

    public async Task ConnectAsync(string msg)
    {
        if (_socket != null)
        {
            await CloseAsync();
            _socket.Dispose();
        }

        _cancellationTokenSource = new CancellationTokenSource();
        InitializeWebSocket(
            "sk-glWCbr31sohrUuM-ls8S1GCm8xD-njSu-p0sk5mneAT3BlbkFJEbdcsNmuL5YLIma1m-05zJdVDxwTHB1E2hM8tfyJsA");

        await _socket.ConnectAsync(_uri, _cancellationTokenSource.Token);
        Console.WriteLine("Connected to OpenAI Realtime API. (from connectAsync)");

        var initialMessage = new
        {
            type = "response.create",
            response = new
            {
                modalities = new[] { "text" },
                instructions =
                    "Ne fillim te bisedes thuaj: 'Pershendetje, si mund t'ju ndihmoj?' si text. Mos perdor format JSON. Je nje doktor. Pergjigju shkurt dhe ne gjuhen qe useri flet me ty. Ne qofte se te duhet te sygjerosh ilace, zgjidh 5 ilace nga te dhenat qe do te kalohen ne prompt dhe refero vetem emrin dhe cmimin." +
                    "Pergjigjet ktheji si nje person real. Edhe nqs te dhenat thone qe nje ilac nuk eshte ne stock, perseri mund ta referosh. Pergjigjet ktheji ne formatin text ose markdown dhe asnjehere ne JSON. I repeat: Do NOT use JSON format!!!"
            }
        };

        var responseCreateMessageJson = JsonSerializer.Serialize(initialMessage);
        var responseCreateBytes = Encoding.UTF8.GetBytes(responseCreateMessageJson);
        await _socket.SendAsync(new ArraySegment<byte>(responseCreateBytes), WebSocketMessageType.Text, true,
            _cancellationTokenSource.Token);
        _ = Task.Run(async () => await ReceiveMessagesAsync(_cancellationTokenSource.Token),
            _cancellationTokenSource.Token);
    }
    
    public async Task PlaceOrder(string name, int quantity, string address, string productNumber)
    {
        Console.WriteLine("order placed" + address + quantity + name + productNumber);
        await _twilioClient.SendMessageAsync(_sessionId,
            "Faleminderit per porosine e kryer, nje motorritst do ju kontaktoje me vone!");
    }
    
    public async Task SendMessageAsync(string message)
    {
        if (_socket.State != WebSocketState.Open)
        {
            await ConnectAsync(message);
        }
        var tools = new object[]
        {
            new
            {
                name = "PlaceOrder",
                type = "function",
                    description = "Gjithmone kerkoj userit te shkruaj 'Konfirmo' para se te therrasesh funksionin funksionin. Mos e thirr funksionin pa thene useri 'Konfirmo'",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            emri = new {
                                type = "string",
                                description = "The order to place"
                            },
                            sasia = new {
                                type = "number",
                                description = "The quantity of the order"
                            },
                            adresa = new {
                                type = "string",
                                description = "The address to deliver the order"
                            }
                        },
                        required = new [] { "emri", "sasia", "adresa" },
                        additionalProperties = false
                    }
            }
        };
        
        var products = await _qdrantService.SearchProducts(message);
        var productsJson = JsonConvert.SerializeObject(products);
        var responseCreateMessage = new
        {
            type = "response.create",
            response = new
            {
                tools = tools,
                modalities = new[] { "text" },
                instructions =
                    "Je nje doktor. Pergjigju shkurt dhe ne gjuhen qe useri flet me ty. Ne qofte se te duhet te sygjerosh ilace, zgjidh 5 ilace nga te dhenat qe do te kalohen ne prompt dhe refero vetem emrin dhe cmimin." +
                    $"Pergjigjet ktheji si nje person real. Edhe nqs te dhenat thone qe nje ilac nuk eshte ne stock, perseri mund ta referosh. Pergjigjet ktheji ne formatin text ose markdown dhe asnjehere ne JSON. Keto jane disa te dhena qe mund te ndihmojne: {productsJson}, perdori vetem kur te nevojiten, nuk eshte e domosdoshme te bazohesh gjithmone ketu." +
                    $" Do NOT use JSON format!!!" +
                    $"Ne qofte se nuk ke informacion rreth ilaceve, mund ti kerkosh dhe njeher userit te shkruaj emrin e sakte te ilacit.'" +
                    "Kur nje person kerkon te beje nje porosi, duhet te kthesh nje forme  e cila kerkon emrin, produktin, sasine dhe adresen."
            }
        };
        
        var messageObject = new
        {
            type = "conversation.item.create",
            item = new
            {
                type = "message",
                role = "user",
                content = new[]
                {
                    new
                    {
                        type = "input_text",
                        text = message
                    }
                }
            }
        };

        var conversationItemMessageJson = JsonSerializer.Serialize(messageObject);
        var conversationItemBytes = Encoding.UTF8.GetBytes(conversationItemMessageJson);
        Console.WriteLine("Sending message: " + conversationItemMessageJson);
        await _socket.SendAsync(new ArraySegment<byte>(conversationItemBytes), WebSocketMessageType.Text, true,
            _cancellationTokenSource.Token);

        var responseCreateMessageJson = JsonSerializer.Serialize(responseCreateMessage);
        var responseCreateBytes = Encoding.UTF8.GetBytes(responseCreateMessageJson);
        await _socket.SendAsync(new ArraySegment<byte>(responseCreateBytes), WebSocketMessageType.Text, true,
            _cancellationTokenSource.Token);
    }

    private async Task ReceiveMessagesAsync(CancellationToken token)
    {
        var buffer = new byte[1024 * 4];
        var messageBuilder = new StringBuilder();

        while (_socket.State == WebSocketState.Open && !token.IsCancellationRequested)
        {
            try
            {
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                var messagePart = Encoding.UTF8.GetString(buffer, 0, result.Count);
                messageBuilder.Append(messagePart);

                if (result.EndOfMessage)
                {
                    var completeMessage = messageBuilder.ToString();
                    messageBuilder.Clear();
                    try
                    {
                        using var jsonDocument = JsonDocument.Parse(completeMessage);
                        if (jsonDocument.RootElement.TryGetProperty("item", out var item))
                        {
                            Console.WriteLine(item);
                            if (item.TryGetProperty("type", out var eventType) && eventType.ToString() == "function_call" && item.TryGetProperty("status", out var status) && status.ToString() == "completed")
                            {
                                var functionName = item.GetProperty("name").GetString();
                                var arguments = item.GetProperty("arguments").GetString();
                                
                                Console.WriteLine($"Function: {functionName}, Arguments: {arguments}");
                                
                                await HandleFunctionCall(functionName, arguments);
                            }
                        }


                        if (jsonDocument.RootElement.TryGetProperty("text", out var textElement))
                        {
                            Console.WriteLine("Received JSON22 text message: " + textElement.GetString());
                            await _twilioClient.SendMessageAsync(_sessionId, textElement.GetString());
                        }
                    }
                    catch (JsonException)
                    {
                        Console.WriteLine("Received plain text message: " + completeMessage);
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
        if (_socket != null && (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseReceived))
        {
            try
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cancellationTokenSource.Token);
                Console.WriteLine("Connection closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing connection: {ex.Message}");
            }
        }

        _cancellationTokenSource.Cancel();
    }
    
    private async Task HandleFunctionCall(string functionName, string argumentsJson)
    {
        try
        {
            switch (functionName)
            {
                case "PlaceOrder":
                    var placeOrderArgs = JsonDocument.Parse(argumentsJson);
                    placeOrderArgs.RootElement.TryGetProperty("emri", out var name);
                    placeOrderArgs.RootElement.TryGetProperty("sasia", out var quantity);
                    placeOrderArgs.RootElement.TryGetProperty("adresa", out var address);
                    placeOrderArgs.RootElement.TryGetProperty("numri", out var productNumber);
                    await PlaceOrder(name.ToString(), int.Parse(quantity.ToString()), address.ToString(), productNumber.ToString());
                    break;

                default:
                    Console.WriteLine($"Unknown function: {functionName}");
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("error calling function, " + e.Message);
            throw;
        }
    }

}