using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Documents;

public sealed record JobApplicationNote
{
    private JobApplicationNote(
        Guid id,
        Guid jobApplicationId,
        Guid organizationId,
        string note,
        Guid createdById,
        UserSnapshot createdBy,
        bool isDeleted,
        DateTimeOffset createdAt)
    {
        Id = id;
        JobApplicationId = jobApplicationId;
        Note = note;
        CreatedById = createdById;
        CreatedBy = createdBy;
        IsDeleted = isDeleted;
        CreatedAt = createdAt;
        OrgId = organizationId;
    }

    public Guid Id { get; }

    public Guid JobApplicationId { get; }
    
    public Guid OrgId { get; }

    public string Note { get; private set; }

    public Guid CreatedById { get; }
    public UserSnapshot CreatedBy { get; }
    public DateTimeOffset? CreatedAt { get; private set; }
    
    public Guid? ModifyById { get; private set; }
    public UserSnapshot? ModifyBy { get; private set; }
    public DateTimeOffset? ModifyAt { get; private set; }

    public bool IsDeleted { get; set; }

    public static JobApplicationNote Create(
        Guid jobApplicationId,
        Guid organizationId,
        ShortNote note,
        Guid createdById,
        UserSnapshot createdBy,
        DateTimeOffset createdAt)

    {
        ArgumentNullException.ThrowIfNull(note);

        return new JobApplicationNote(
            Guid.NewGuid(),
            jobApplicationId,
            organizationId,
            note.Value,
            createdById,
            createdBy,
            false, createdAt);
    }

    public JobApplicationNote Delete()
    {
        return this with
        {
            IsDeleted = true
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