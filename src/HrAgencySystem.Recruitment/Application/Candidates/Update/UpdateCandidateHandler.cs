using HrAgencySystem.Recruitment.Application.Candidates.Create;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.Candidates.Update;

public static class UpdateCandidateHandler
{

    [AggregateHandler]
    public static async Task<(CandidateUpdated, Wolverine.Marten.Events)>
        Handle(UpdateCandidate command, Candidate aggregate,
            IRecruitmentService service,
            IClock clock, CancellationToken ct)
    {

        var (data, _) = CandidateDataFactory.Create(command);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);
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
}