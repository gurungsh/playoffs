using System.Security.Cryptography;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Playoffs.Contracts.Tournaments.V1;
using Testcontainers.MongoDb;

namespace Playoffs.Tournament.Tests;

public sealed class TournamentFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string Issuer = "test-issuer";
    private static readonly SymmetricSecurityKey Key = new(RandomNumberGenerator.GetBytes(32));
    private readonly MongoDbContainer _mongo = new MongoDbBuilder().WithImage("mongo:8").Build();

    public Task InitializeAsync() => _mongo.StartAsync();

    async Task IAsyncLifetime.DisposeAsync() => await _mongo.DisposeAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // UseSetting, not ConfigureAppConfiguration: Program.cs reads config before Build()
        builder.UseSetting("Mongo:ConnectionString", _mongo.GetConnectionString());
        builder.UseSetting("Mongo:Database", $"t{Guid.NewGuid():N}");
        builder.ConfigureTestServices(services =>
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                o =>
                {
                    // Replace the Keycloak metadata fetch with a static config and our own key.
                    o.ConfigurationManager =
                        new StaticConfigurationManager<OpenIdConnectConfiguration>(
                            new OpenIdConnectConfiguration { Issuer = Issuer }
                        );
                    o.TokenValidationParameters.ValidIssuer = Issuer;
                    o.TokenValidationParameters.IssuerSigningKey = Key;
                }
            )
        );
    }

    public TournamentService.TournamentServiceClient CreateClient2() =>
        new(
            GrpcChannel.ForAddress(
                Server.BaseAddress,
                new GrpcChannelOptions { HttpHandler = Server.CreateHandler() }
            )
        );

    public static Metadata BearerFor(string sub) =>
        new()
        {
            {
                "Authorization",
                "Bearer "
                    + new JsonWebTokenHandler().CreateToken(
                        new SecurityTokenDescriptor
                        {
                            Issuer = Issuer,
                            Audience = "playoffs-tournament",
                            Claims = new Dictionary<string, object> { ["sub"] = sub },
                            Expires = DateTime.UtcNow.AddMinutes(5),
                            SigningCredentials = new SigningCredentials(
                                Key,
                                SecurityAlgorithms.HmacSha256
                            ),
                        }
                    )
            },
        };
}