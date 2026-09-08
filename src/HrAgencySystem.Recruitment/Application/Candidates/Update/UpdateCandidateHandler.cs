using System.Data.Common;
using HrAgencySystem.Recruitment.Application.Candidates.Create;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.Candidates.Update;

public static class UpdateCandidateHandler
{

    [AggregateHandler]
    public static async Task<(CandidateUpdated, Wolverine.Marten.Events)>
        Handle(UpdateCandidate command, Candidate aggregate,
            IUserSnapshotRepository snapshotRepository,
            IClock clock, CancellationToken ct)
    {

        var (data, _) = CandidateDataFactory.Create(command);
        var user = await GetUser(snapshotRepository, command.ModifiedBy, ct);
        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
        
        var @event = new CandidateUpdated(command.CandidateId, command.OrganizationId,
            data.Phone.Value,
            data.FirstName.Value,
            data.LastName.Value,
            data.Note.Value,
            user,
            clock.UtcNow);

        return (@event, [@event]);
    }
    
    private static async Task<UserSnapshot> GetUser(IUserSnapshotRepository repository, Guid userId,
        CancellationToken ct)
    {
        var user = await repository.GetUserAsync(userId, ct);
        return user ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }
}