using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Playoffs.Tournament.Data;

public sealed class TournamentDocument
{
    [BsonId]
    public required string Id { get; init; }
    public required string OrganizerId { get; init; }
    public required string Name { get; init; }
    public required string SportId { get; init; }

    [BsonRepresentation(BsonType.String)]
    public required TournamentStatus Status { get; init; }

    public required int MaxTeams { get; init; }
    public bool IsDeleted { get; init; }
    public int Version { get; init; }
    public List<TeamDocument> Teams { get; init; } = [];
}