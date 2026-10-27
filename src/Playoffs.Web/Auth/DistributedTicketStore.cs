using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace Playoffs.Web.Auth;

public sealed class DistributedTicketStore(IDistributedCache cache) : ITicketStore
{
    private const string KeyPrefix = "ticket:";

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket);
        return key;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket) =>
        cache.SetAsync(
            KeyPrefix + key,
            TicketSerializer.Default.Serialize(ticket),
            new DistributedCacheEntryOptions { AbsoluteExpiration = ticket.Properties.ExpiresUtc }
        );

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var bytes = await cache.GetAsync(KeyPrefix + key);
        return bytes is null ? null : TicketSerializer.Default.Deserialize(bytes);
    }

    public Task RemoveAsync(string key) => cache.RemoveAsync(KeyPrefix + key);
}