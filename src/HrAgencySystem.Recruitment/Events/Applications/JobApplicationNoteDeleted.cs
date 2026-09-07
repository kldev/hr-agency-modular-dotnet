using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationNoteDeleted(Guid JobApplicationId,
    Guid CandidateId,
    UserSnapshot DeletedBy, 
    DateTimeOffset DeletedAt);