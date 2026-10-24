using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Playoffs.Contracts.Tournaments.V1;
using Playoffs.Tournament.Data;
using TournamentStatus = Playoffs.Contracts.Tournaments.V1.TournamentStatus;

namespace Playoffs.Tournament.Tests;

public sealed class TournamentApiTests(TournamentFactory factory) : IClassFixture<TournamentFactory>
{
    private IMongoCollection<TournamentDocument> Collection =>
        factory.Services.GetRequiredService<IMongoCollection<TournamentDocument>>();

    [Fact]
    public async Task Authenticated_call_returns_a_seeded_tournament()
    {
        await SeedLoader.RunAsync(Collection);
        var client = factory.CreateClient2();

        var response = await client.GetTournamentAsync(
            new GetTournamentRequest { Id = SeedLoader.SummerCupId },
            TournamentFactory.BearerFor(SeedLoader.Organizer1Id)
        );

        Assert.Equal("Summer Cup", response.Tournament.Name);
        Assert.Equal(3, response.Tournament.Teams.Count);
        Assert.Equal(TournamentStatus.RegistrationOpen, response.Tournament.Status);
    }

    [Fact]
    public async Task Anonymous_call_is_rejected()
    {
        var client = factory.CreateClient2();

        var ex = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.ListTournamentsAsync(new ListTournamentsRequest())
        );

        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [Fact]
    public async Task Unknown_tournament_is_not_found()
    {
        var client = factory.CreateClient2();

        var ex = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.GetTournamentAsync(
                new GetTournamentRequest { Id = Guid.NewGuid().ToString() },
                TournamentFactory.BearerFor(SeedLoader.Organizer1Id)
            )
        );

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task Seed_loader_run_twice_changes_nothing()
    {
        await SeedLoader.RunAsync(Collection);
        var before = await Collection
            .Find(FilterDefinition<TournamentDocument>.Empty)
            .ToListAsync();

        await SeedLoader.RunAsync(Collection);
        var after = await Collection.Find(FilterDefinition<TournamentDocument>.Empty).ToListAsync();

        Assert.Equal(2, after.Count);
        Assert.Equal(before.Select(d => d.Id).Order(), after.Select(d => d.Id).Order());
    }
}