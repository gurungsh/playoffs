using System.Net;

namespace Playoffs.Web.Tests;

public sealed class AuthorizationTests(WebFactory factory) : IClassFixture<WebFactory>
{
    [Fact]
    public async Task Home_page_is_public()
    {
        var response = await factory
            .CreateClientWithRoles(null)
            .GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_user_is_redirected_to_keycloak()
    {
        var response = await factory
            .CreateClientWithRoles(null)
            .GetAsync(new Uri("/Organizer", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location!.ToString();
        Assert.StartsWith(WebFactory.AuthorizationEndpoint, location, StringComparison.Ordinal);
        Assert.Contains("code_challenge=", location, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Organizer", "/Organizer")]
    [InlineData("Admin", "/Organizer")]
    [InlineData("Admin", "/Admin")]
    public async Task User_with_role_reaches_the_area(string roles, string path)
    {
        var response = await factory
            .CreateClientWithRoles(roles)
            .GetAsync(new Uri(path, UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("Referee", "/Organizer")]
    [InlineData("Organizer", "/Admin")]
    public async Task User_without_role_is_sent_to_access_denied(string roles, string path)
    {
        var client = factory.CreateClientWithRoles(roles);

        var response = await client.GetAsync(new Uri(path, UriKind.Relative));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/AccessDenied", response.Headers.Location!.AbsolutePath);

        var denied = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
    }

    [Fact]
    public async Task Logout_without_antiforgery_token_is_rejected()
    {
        var response = await factory
            .CreateClientWithRoles("Organizer")
            .PostAsync(new Uri("/Account/Logout", UriKind.Relative), null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}