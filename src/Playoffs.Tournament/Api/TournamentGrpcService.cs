using Grpc.Core;
using MongoDB.Driver;
using Playoffs.Tournament.Data;
using Proto = Playoffs.Contracts.Tournaments.V1;

namespace Playoffs.Tournament.Api;

public sealed class TournamentGrpcService(IMongoCollection<TournamentDocument> tournaments)
    : Proto.TournamentService.TournamentServiceBase
{
    public override async Task<Proto.GetTournamentResponse> GetTournament(
        Proto.GetTournamentRequest request,
        ServerCallContext context
    )
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be a guid."));
        }

        var key = id.ToString();
        var doc =
            await tournaments
                .Find(t => t.Id == key && !t.IsDeleted)
                .FirstOrDefaultAsync(context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Tournament not found."));

        return new Proto.GetTournamentResponse { Tournament = ToProto(doc) };
    }

    public override async Task<Proto.ListTournamentsResponse> ListTournaments(
        Proto.ListTournamentsRequest request,
        ServerCallContext context
    )
    {
        var docs = await tournaments.Find(t => !t.IsDeleted).ToListAsync(context.CancellationToken);
        var response = new Proto.ListTournamentsResponse();
        response.Tournaments.AddRange(docs.Select(ToProto));

        return response;
    }

    private static Proto.Tournament ToProto(TournamentDocument doc) =>
        new()
        {
            Id = doc.Id,
            OrganizerId = doc.OrganizerId,
            Name = doc.Name,
            SportId = doc.SportId,
            Status = doc.Status switch
            {
                TournamentStatus.Draft => Proto.TournamentStatus.Draft,
                TournamentStatus.RegistrationOpen => Proto.TournamentStatus.RegistrationOpen,
                TournamentStatus.Started => Proto.TournamentStatus.Started,
                _ => Proto.TournamentStatus.Unspecified,
            },
            MaxTeams = doc.MaxTeams,
            Teams = { doc.Teams.Select(t => new Proto.Team { Id = t.Id, Name = t.Name }) },
        };
}