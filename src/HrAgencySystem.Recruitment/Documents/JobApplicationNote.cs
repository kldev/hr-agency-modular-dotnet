using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Documents;

public sealed record JobApplicationNote( 
    Guid Id,
    Guid JobApplicationId,
    Guid OrgId,
    Guid CandidateId,
    string Note,
    Guid CreatedById,
    UserSnapshot CreatedBy,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    Guid? ModifyById,
    UserSnapshot? ModifyBy,
    DateTimeOffset? ModifyAt)
{
    public static JobApplicationNote Create(
        Guid jobApplicationId,
        Guid organizationId,
        Guid candidateId,
        ShortNote note,
        UserSnapshot createdBy,
        DateTimeOffset createdAt)

    {
        ArgumentNullException.ThrowIfNull(note);

        return new JobApplicationNote(
            Guid.NewGuid(),
            jobApplicationId,
            organizationId,
            candidateId,
            note.Value,
            createdBy.Id,
            createdBy,
            false, 
            createdAt,
            null,
            null,
            null);
    }

    public JobApplicationNote Delete( 
        UserSnapshot deleteBy,
        DateTimeOffset deleteAt)
    {
        return this with
        {
            IsDeleted = true,
            ModifyAt = deleteAt,
            ModifyBy = deleteBy,
            ModifyById = deleteBy.Id
        };
    }

    public JobApplicationNote Modify(ShortNote note,
        Guid modifyById,
        UserSnapshot modifyBy,
        DateTimeOffset modifyAt)
    {
        return this with
        {
            ModifyAt = modifyAt,
            ModifyBy = modifyBy,
            ModifyById = modifyById, 
            Note = note.Value
        };
    }
}