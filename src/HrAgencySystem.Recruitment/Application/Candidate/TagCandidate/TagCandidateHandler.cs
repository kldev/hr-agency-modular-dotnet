using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;
namespace HrAgencySystem.Recruitment.Application.Candidate.TagCandidate;

// ReSharper disable once UnusedType.Global
public static class TagCandidateHandler
{
    [AggregateHandler]
    public static async Task<(CandidateTagged, Wolverine.Marten.Events)> Handle(
        TagCandidate command, 
        Domain.Candidates.Candidate aggregate,
        ITagRepository tagRepository,
        IUserSnapshotRepository snapshotRepository,
        IClock clock,
        CancellationToken ct)
    {
        
        var tag = await tagRepository.GetTag(command.TagId, ct);
        var user = await GetCreatedBy(snapshotRepository, command.CreatedBy, ct);

        var @event = new CandidateTagged(aggregate.Id.Value, tag, user, clock.UtcNow);
        
        return (@event, [@event]);
    }

    private static async Task<UserSnapshot> GetCreatedBy(IUserSnapshotRepository snapshotRepository, Guid createdById, 
        CancellationToken ct)
    {
        var user = await snapshotRepository.GetUserAsync(createdById, ct);
        return user ?? throw new NotFoundException(IUserSnapshotRepository.NotFoundMessage);
    }
}