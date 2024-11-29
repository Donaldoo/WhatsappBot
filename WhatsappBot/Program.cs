using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WhatsappBot.Data;
using WhatsappBot.Services;
using JsonElement = System.Text.Json.JsonElement;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddEntityFrameworkNpgsql().AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<WebScraperService>();
builder.Services.AddSingleton<TwilioMessageService>();
builder.Services.AddScoped<OpenAiService>();
builder.Services.AddScoped<QdrantService>();
builder.Services.AddSingleton<OpenAiSessionManager>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();
app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path.ToString().Contains("/media-stream"))
    {
        Console.WriteLine("Media stream request.");
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await HandleWebSocketConnection(webSocket);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }
});

app.MapPost("/incoming-call", () =>
{
    return Results.Text("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                        "<Response>" +
                        "<Say>Please wait while we connect your call to the A. I. voice assistant, powered by Twilio and the Open-A.I. Realtime API</Say>" +
                        "<Pause length=\"1\"/>" +
                        "<Say>O.K. you can start talking!</Say>" +
                        "<Connect>" +
                        $"<Stream url=\"wss://4472-80-78-71-113.ngrok-free.app/media-stream\" track=\"inbound\" />" +
                        "</Connect>" +
                        "</Response>", contentType: "application/xml");
});

app.UseAuthorization();

app.MapControllers();

app.Run();

async Task HandleWebSocketConnection(WebSocket clientSocket)
{
    Console.WriteLine("Client connected.");

    using var openAiSocket = new ClientWebSocket();
    var openAiUri = new Uri("wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01");
    openAiSocket.Options.SetRequestHeader("Authorization", $"Bearer sk-glWCbr31sohrUuM-ls8S1GCm8xD-njSu-p0sk5mneAT3BlbkFJEbdcsNmuL5YLIma1m-05zJdVDxwTHB1E2hM8tfyJsA");
    openAiSocket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");

    await openAiSocket.ConnectAsync(openAiUri, CancellationToken.None);
    Console.WriteLine("Connected to the OpenAI Realtime API.");

    var clientBuffer = new byte[4096];
    var openAiBuffer = new byte[4096];
    var clientMessageBuilder = new StringBuilder();
    var openAiMessageBuilder = new StringBuilder();
    string streamSid = null;

    async Task SendSessionUpdate()
    {
        var sessionUpdate = new
        {
            type = "session.update",
            session = new
            {
                turn_detection = new { type = "server_vad" },
                input_audio_format = "g711_ulaw",
                output_audio_format = "g711_ulaw",
                voice = "alloy",
                instructions = "Respond in a friendly and helpful manner.",
                modalities = new[] { "text", "audio" },
                temperature = 0.8
            }
        };

        var sessionUpdateJson = JsonSerializer.Serialize(sessionUpdate);
        Console.WriteLine("Sending session update: " + sessionUpdateJson);
        await openAiSocket.SendAsync(Encoding.UTF8.GetBytes(sessionUpdateJson), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    // OpenAI receive loop
    _ = Task.Run(async () =>
    {
        while (openAiSocket.State == WebSocketState.Open)
        {
            var segment = new ArraySegment<byte>(openAiBuffer);
            try
            {
                var result = await openAiSocket.ReceiveAsync(segment, CancellationToken.None);
                openAiMessageBuilder.Append(Encoding.UTF8.GetString(openAiBuffer, 0, result.Count));

                if (result.EndOfMessage)
                {
                    var completeMessage = openAiMessageBuilder.ToString();
                    openAiMessageBuilder.Clear();
                    try
                    {
                        var parsedResponse = JsonSerializer.Deserialize<JsonElement>(completeMessage);

                        if (parsedResponse.GetProperty("type").GetString() == "response.audio.delta" &&
                            parsedResponse.TryGetProperty("delta", out var delta))
                        {
                            var audioDelta = new
                            {
                                @event = "media",
                                streamSid,
                                media = new { payload = Convert.FromBase64String(delta.GetString()!) }
                            };
                            var audioDeltaJson = JsonSerializer.Serialize(audioDelta);
                            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(audioDeltaJson), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing OpenAI message: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving from OpenAI: " + ex.Message);
            }
        }
    });

    await SendSessionUpdate();

    // Client receive loop
    while (clientSocket.State == WebSocketState.Open)
    {
        var segment = new ArraySegment<byte>(clientBuffer);
        var result = await clientSocket.ReceiveAsync(segment, CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close) break;

        clientMessageBuilder.Append(Encoding.UTF8.GetString(clientBuffer, 0, result.Count));

        if (result.EndOfMessage)
        {
            var completeMessage = clientMessageBuilder.ToString();
            clientMessageBuilder.Clear();
            try
            {
                var parsedMessage = JsonSerializer.Deserialize<JsonElement>(completeMessage);
                Console.WriteLine("parsedMsg:" + parsedMessage);
                
                if (parsedMessage.GetProperty("event").GetString() == "media" &&
                    parsedMessage.TryGetProperty("media", out var media) &&
                    media.TryGetProperty("payload", out var payload))
                {
                    
                    byte[] audioBuffer = Convert.FromBase64String(payload.GetString());
                    Console.WriteLine($"Decoded Audio Buffer Length: {audioBuffer}");
                    
                    var audioAppend = new
                    {
                        type = "input_audio_buffer.append",
                        audio = audioBuffer
                    };
                    var audioAppendJson = JsonSerializer.Serialize(audioAppend);
                    await openAiSocket.SendAsync(Encoding.UTF8.GetBytes(audioAppendJson), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else if (parsedMessage.GetProperty("event").GetString() == "start")
                {
                    streamSid = parsedMessage.GetProperty("start").GetProperty("streamSid").GetString();
                    Console.WriteLine("Incoming stream started: " + streamSid);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing client message: " + ex.Message);
            }
        }
    }

    // Close OpenAI connection
    if (openAiSocket.State == WebSocketState.Open)
    {
        await openAiSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None);
    }

    Console.WriteLine("Client disconnected.");
}
