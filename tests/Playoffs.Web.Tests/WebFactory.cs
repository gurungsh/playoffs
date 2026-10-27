using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Playoffs.Web.Tests;

public sealed class WebFactory : WebApplicationFactory<Program>
{
    public const string AuthorizationEndpoint = "https://keycloak.test/auth";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Auth:ClientSecret", "test-secret");
        builder.ConfigureTestServices(services =>
        {
            // Signed-in users come from a header; anonymous requests still challenge through OIDC.
            services
                .AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, HeaderAuthHandler>(
                    HeaderAuthHandler.SchemeName,
                    null
                );
            services.PostConfigure<AuthenticationOptions>(o =>
                o.DefaultAuthenticateScheme = HeaderAuthHandler.SchemeName
            );
            services.PostConfigure<OpenIdConnectOptions>(
                OpenIdConnectDefaults.AuthenticationScheme,
                o =>
                {
                    // Replace the Keycloak metadata fetch with a static config.
                    o.Configuration = new OpenIdConnectConfiguration
                    {
                        AuthorizationEndpoint = AuthorizationEndpoint,
                    };
                    o.ConfigurationManager =
                        new StaticConfigurationManager<OpenIdConnectConfiguration>(o.Configuration);
                }
            );
        });
    }

    public HttpClient CreateClientWithRoles(string? roles)
    {
        var client = CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );
        if (roles is not null)
        {
            client.DefaultRequestHeaders.Add(HeaderAuthHandler.RolesHeader, roles);
        }
        return client;
    }
}

public sealed class HeaderAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Header";
    public const string RolesHeader = "X-Test-Roles";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(RolesHeader, out var roles))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = roles
            .ToString()
            .Split(',')
            .Select(role => new Claim("roles", role))
            .Append(new Claim("preferred_username", "test-user"));
        var identity = new ClaimsIdentity(claims, SchemeName, "preferred_username", "roles");
        return Task.FromResult(
            AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)
            )
        );
    }
}