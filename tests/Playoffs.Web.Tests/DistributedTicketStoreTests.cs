using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Playoffs.Web.Auth;

namespace Playoffs.Web.Tests;

public sealed class DistributedTicketStoreTests
{
    private readonly DistributedTicketStore _store = new(
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()))
    );

    [Fact]
    public async Task Ticket_with_tokens_round_trips_and_can_be_removed()
    {
        var properties = new AuthenticationProperties();
        properties.StoreTokens([new AuthenticationToken { Name = "access_token", Value = "abc" }]);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "u1")], "test"));

        var key = await _store.StoreAsync(
            new AuthenticationTicket(principal, properties, "Cookies")
        );
        var stored = await _store.RetrieveAsync(key);
        await _store.RemoveAsync(key);

        Assert.Equal("abc", stored!.Properties.GetTokenValue("access_token"));
        Assert.Equal("u1", stored.Principal.FindFirst("sub")!.Value);
        Assert.Null(await _store.RetrieveAsync(key));
    }
}