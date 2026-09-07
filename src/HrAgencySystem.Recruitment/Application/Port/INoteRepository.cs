using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.Port;

public interface INoteRepository
{
    Task CreateNoteAsync(CreateNoteDocument noteDocument, UserSnapshot user);
}

// ReSharper disable once ClassNeverInstantiated.Global
public record CreateNoteDocument(Guid JobApplicationId, Guid OrganizationId, Guid CandidateId, ShortNote Text);
