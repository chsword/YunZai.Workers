using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using YunZai.Workers.AspireApp.Unsplash.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<UnsplashService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/image/{keyword}/{width:int}/{height:int}", async (string keyword, int width, int height) =>
    {
        //get UnsplashService
        var unsplashService = app.Services.GetRequiredService<UnsplashService>();
        var imageUrl = await unsplashService.GetImageUrlAsync(keyword);
        if (string.IsNullOrEmpty(imageUrl))
            return Results.NotFound(new {Message = "No images found for the given keyword."});
        // image url + width + height with query string
        imageUrl = $"{imageUrl}&w={width}&h={height}";
        var client = new HttpClient();
        var response = await client.GetByteArrayAsync(imageUrl);
        //return Results.Bytes(response, "image/jpeg", $"image_{keyword}.jpg");
        return Results.File(response, "image/jpeg");

    })
    .WithName("unsplashImage");

app.MapDefaultEndpoints();

app.Run();
