
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WhatsappBot;
using WhatsappBot.Data;
using WhatsappBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddEntityFrameworkNpgsql().AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<WebScraperService>();
builder.Services.AddScoped<PhoneNumberService>();
builder.Services.AddScoped<CampgaignService>();
builder.Services.AddSingleton<TwilioMessageService>();
builder.Services.AddScoped<OpenAiService>();
builder.Services.AddScoped<QdrantService>();
builder.Services.AddSingleton<OpenAiSessionManager>();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowFrontendApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();