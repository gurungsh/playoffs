using Microsoft.Extensions.Options;

namespace Playoffs.Web.Tests;

public sealed class StartupTests
{
    [Fact]
    public void Missing_client_secret_fails_at_startup()
    {
        using var baseFactory = new WebFactory();
        using var factory = baseFactory.WithWebHostBuilder(b =>
            b.UseSetting("Auth:ClientSecret", "")
        );

        var ex = Assert.Throws<OptionsValidationException>(() => factory.CreateClient());

        Assert.Contains("ClientSecret", ex.Message, StringComparison.Ordinal);
    }
}