using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationNoteAdded(Guid JobApplicationId,
    Guid CandidateId,
    DateTimeOffset OccurredAt,
    string Note,
    UserSnapshot Author) : IJobApplicationEvent;