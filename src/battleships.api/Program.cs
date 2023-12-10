using battleships.api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GameManagerOptions>(_ =>
{
    var options = new GameManagerOptions();
    if (!int.TryParse(Environment.GetEnvironmentVariable("MAP_COUNT"), out var mapCount))
        mapCount = 200;
    options.MapCount = mapCount;
    
    return options;
});

builder.Services.AddSingleton<IGameManager, GameManager>();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Enter your token here. Can be anything how you want to identify your game.",
        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    setup.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/fire", (
            [FromQuery(Name = "test")] bool? isSimulation,
            [FromHeader(Name = "Authorization")] string authHeader,
            [FromServices] IGameManager gameManager) =>
        gameManager.Fire(new FireRequest(isSimulation ?? false, authHeader)))
    .WithName("status")
    .WithOpenApi();

app.MapGet("/fire/{row}/{column}", (
            [FromRoute] int row,
            [FromRoute] int column,
            [FromQuery(Name = "test")] bool? isSimulation,
            [FromHeader(Name = "Authorization")] string authHeader,
            [FromServices] IGameManager gameManager) =>
        gameManager.Fire(new FireRequest(isSimulation ?? false, authHeader, row, column)))
    .WithName("fire")
    .WithOpenApi();

app.MapGet("/fire/{row}/{column}/avenger/{avenger}", (
            [FromRoute] int row,
            [FromRoute] int column,
            [FromRoute] string avenger,
            [FromQuery(Name = "test")] bool? isSimulation,
            [FromHeader(Name = "Authorization")] string authHeader,
            [FromServices] IGameManager gameManager) =>
        gameManager.FireWithAvenger(new FireRequest(isSimulation ?? false, authHeader, row, column, avenger)))
    .WithName("fire-with-avenger")
    .WithOpenApi();

app.MapGet("/reset", (
            [FromQuery(Name = "test")] bool? isSimulation,
            [FromHeader(Name = "Authorization")] string authHeader,
            [FromServices] IGameManager gameManager) =>
        gameManager.Reset(new ResetRequest(isSimulation ?? false, authHeader)))
    .WithName("reset")
    .WithOpenApi();

app.MapGet("/status", (
            [FromQuery(Name = "test")] bool? isSimulation,
            [FromHeader(Name = "Authorization")] string authHeader,
            [FromServices] IGameManager gameManager) =>
        gameManager.Status(new StatusRequest(isSimulation ?? false, authHeader)))
    .WithName("game-status")
    .WithOpenApi();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapHub<MatchHub>("/matchHub");

app.Run();