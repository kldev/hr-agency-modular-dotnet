using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Events.Candidate;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;
using D = HrAgencySystem.Recruitment.Domain.Candidate;
namespace HrAgencySystem.Recruitment.Application.Candidate.TagCandidate;

public static class TagCandidateHandler
{
    [AggregateHandler]
    public static async Task<(CandidateTagged, Wolverine.Marten.Events)> Handle(
        TagCandidate command, 
        D.Candidate aggregate,
        ITagRepository tagRepository,
        IUserSnapshotRepository snapshotRepository,
        IClock clock,
        CancellationToken ct)
    {
        var tag = await tagRepository.GetTag(command.TagId, ct);
        var user = await GetCreatedBy(snapshotRepository, command.CreatedBy, ct);

        var @event = new CandidateTagged(command.CandidateId, tag, user, clock.UtcNow);
        
        return (@event, [@event]);
    }

    private static async Task<UserSnapshot> GetCreatedBy(IUserSnapshotRepository snapshotRepository, Guid createdById, 
        CancellationToken ct)
    {
        var user = await snapshotRepository.GetUserAsync(createdById, ct);
        return user ?? throw new NotFoundException(IUserSnapshotRepository.NotFoundMessage);
    }
}