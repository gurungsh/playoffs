using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using Playoffs.Tournament.Api;
using Playoffs.Tournament.Data;
using Serilog;

MongoSetup.RegisterConventions();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (ctx, cfg) =>
        cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
);

var mongo = builder.Configuration.GetSection("Mongo");
var mongoConnectionString =
    mongo["ConnectionString"]
    ?? throw new InvalidOperationException("Config 'Mongo:ConnectionString' is missing.");
var mongoDatabase =
    mongo["Database"] ?? throw new InvalidOperationException("Config 'Mongo:Database' is missing.");

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>()
        .GetDatabase(mongoDatabase)
        .GetCollection<TournamentDocument>(MongoSetup.TournamentsCollection)
);

var auth = builder.Configuration.GetSection("Auth");
var authority =
    auth["Authority"] ?? throw new InvalidOperationException("Config 'Auth:Authority' is missing.");
var audience =
    auth["Audience"] ?? throw new InvalidOperationException("Config 'Auth:Audience' is missing.");

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = auth.GetValue("RequireHttpsMetadata", true);
    });

builder.Services.AddAuthorization();

builder.Services.AddGrpc();
builder.Services.AddHealthChecks();

var app = builder.Build();

var tournaments = app.Services.GetRequiredService<IMongoCollection<TournamentDocument>>();
await MongoSetup.EnsureIndexesAsync(tournaments);
if (app.Configuration.GetValue<bool>("Seed:Enabled"))
{
    await SeedLoader.RunAsync(tournaments);
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapGrpcService<TournamentGrpcService>().RequireAuthorization();

app.Run();

public partial class Program;