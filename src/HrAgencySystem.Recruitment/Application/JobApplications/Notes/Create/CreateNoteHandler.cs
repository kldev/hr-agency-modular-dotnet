using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Notes.Create;

// ReSharper disable once UnusedType.Global
public static class CreateNoteHandler
{
    public static async Task<JobApplicationNoteAdded> Handle(
        CreateNote command,
        IRecruitmentService service,
        IClock clock,
        INoteRepository noteRepository,
        CancellationToken ct)
    {
        var user = await service.GetUserAsync(command.AddedBy, ct);
        var application = await service.GetApplicationAsync(command.JobApplicationId, command.OrganizationId, ct);

        var (shortNote, error) = ShortNote.TryCreate(command.Text);
        if (error != null) throw new ValidationException(error);

        var @event = new JobApplicationNoteAdded(
            application.JobApplicationId, 
            application.CandidateId, 
            clock.UtcNow,
            shortNote!.Value, user);

        var createNote = new CreateNoteDocument(
            application.JobApplicationId, 
            application.OrganizationId,
            application.CandidateId,
            shortNote!);

        await noteRepository.CreateNoteAsync(createNote, user);

        return @event;
    }
}