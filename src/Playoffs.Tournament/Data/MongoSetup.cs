using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Playoffs.Tournament.Data;

public static class MongoSetup
{
    public const string TournamentsCollection = "tournaments";

    public static void RegisterConventions() =>
        ConventionRegistry.Register(
            "camelCase",
            new ConventionPack { new CamelCaseElementNameConvention() },
            _ => true
        );

    public static Task EnsureIndexesAsync(IMongoCollection<TournamentDocument> tournaments) =>
        tournaments.Indexes.CreateOneAsync(
            new CreateIndexModel<TournamentDocument>(
                Builders<TournamentDocument>
                    .IndexKeys.Ascending(t => t.OrganizerId)
                    .Ascending(t => t.Status)
            )
        );
}