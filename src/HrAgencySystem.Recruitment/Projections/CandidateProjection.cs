using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Events.Candidate;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Projections;

public sealed record CandidateProjection(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid Id,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid OrganizationId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Email,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string PhoneNumber,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CandidateSource Source,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CandidateStatus Status,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset CreatedAt,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot? CreatedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid? CreatedById,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot? ModifiedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid? ModifyById,
    IReadOnlyList<Tag> Tags
)
{
    public static CandidateProjection Create(
        CandidateCreated @event)
    {
        return new CandidateProjection(@event.Id,
            @event.OrganizationId,
            @event.Email,
            @event.PhoneNumber,
            @event.Source,
            CandidateStatus.Active,
            @event.CreatedAt,
            @event.CreatedBy,
            @event.CreatedBy?.Id,
            null,
            null,
            []);
    }

    public CandidateProjection Apply(CandidateTagged @event)
    {
        var tags = Tags
            .Append(@event.Tag)
            .ToArray();
        
        return this with
        {
            ModifiedBy = @event.Author,
            ModifyById = @event.Author.Id,
            Tags = tags
        };
    }

    public CandidateProjection Apply(CandidateTagRemoved @event)
    {
        var tags = Tags.Where(z => z.Id != @event.Tag.Id).ToArray();
        return this with
        {
            ModifiedBy = @event.Author,
            ModifyById = @event.Author.Id,
            Tags = tags
        };
    }
}