using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
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

builder
    .Services.AddOptions<MongoOptions>()
    .BindConfiguration(MongoOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(
    sp.GetRequiredService<IOptions<MongoOptions>>().Value.ConnectionString
));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>()
        .GetDatabase(sp.GetRequiredService<IOptions<MongoOptions>>().Value.Database)
        .GetCollection<TournamentDocument>(MongoSetup.TournamentsCollection)
);

builder
    .Services.AddOptions<AuthOptions>()
    .BindConfiguration(AuthOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder
    .Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<AuthOptions>>(
        (options, auth) =>
        {
            options.Authority = auth.Value.Authority;
            options.Audience = auth.Value.Audience;
            options.RequireHttpsMetadata = auth.Value.RequireHttpsMetadata;
        }
    );

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