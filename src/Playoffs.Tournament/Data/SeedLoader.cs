using MongoDB.Driver;

namespace Playoffs.Tournament.Data;

public static class SeedLoader
{
    public const string Organizer1Id = "00000000-0000-4000-8000-000000000002";
    public const string Organizer2Id = "00000000-0000-4000-8000-000000000003";
    public const string SummerCupId = "10000000-0000-4000-8000-000000000001";
    public const string WinterLeagueId = "10000000-0000-4000-8000-000000000002";

    public static async Task RunAsync(IMongoCollection<TournamentDocument> tournaments)
    {
        foreach (var doc in Documents)
        {
            if (await tournaments.Find(t => t.Id == doc.Id).AnyAsync())
            {
                continue;
            }

            await tournaments.InsertOneAsync(doc);
        }
    }

    private static TeamDocument Team(string n, string name) =>
        new() { Id = $"20000000-0000-4000-8000-0000000000{n}", Name = name };

    private static readonly TournamentDocument[] Documents =
    [
        new()
        {
            Id = SummerCupId,
            OrganizerId = Organizer1Id,
            Name = "Summer Cup",
            SportId = "soccer",
            Status = TournamentStatus.RegistrationOpen,
            MaxTeams = 8,
            Teams =
            [
                Team("01", "Riverside FC"),
                Team("02", "Hilltop United"),
                Team("03", "Harbor Town"),
            ],
        },
        new()
        {
            Id = WinterLeagueId,
            OrganizerId = Organizer2Id,
            Name = "Winter League",
            SportId = "soccer",
            Status = TournamentStatus.Draft,
            MaxTeams = 6,
        },
    ];
}