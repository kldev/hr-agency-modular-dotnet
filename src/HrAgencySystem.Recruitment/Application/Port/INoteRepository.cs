using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.Port;

public interface INoteRepository
{
    Task CreateNoteAsync(CreateNote note, CancellationToken ct);
}

// ReSharper disable once ClassNeverInstantiated.Global
public record CreateNote(Guid JobApplicationId, Guid OrganizationId, Guid CandidateId, ShortNote Text, Guid AuthorId);
