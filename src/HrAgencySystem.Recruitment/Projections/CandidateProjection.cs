using System.Text.Json;
using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Events.Candidate;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Projections;

public sealed record CandidateProjection(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid Id,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid OrgId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Email,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string PhoneNumber,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string FirstName,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string LastName,
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
    IReadOnlyList<Tag> Tags,
    IReadOnlyList<Guid> TagsIds,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    IReadOnlyList<Guid> CompanyIds)
{
    public static CandidateProjection Create(
        CandidateCreated @event)
    {
        return new CandidateProjection(@event.CandidateId,
            @event.OrganizationId,
            @event.Email,
            @event.Phone,
            @event.FirstName,
            @event.LastName,
            @event.Source,
            CandidateStatus.Active,
            @event.CreatedAt,
            @event.CreatedBy,
            @event.CreatedBy?.Id,
            null,
            null,
            [],
            [], 
            @event.CompanyId.HasValue ? [@event.CompanyId.Value] : []);
    }

    public CandidateProjection Apply(CandidateTagged @event)
    {
        if (TagsIds.Contains(@event.Tag.Id)) return this;
        
        var tags = Tags
            .Append(@event.Tag)
            .ToArray();
        var tagIds = TagsIds.Append(@event.Tag.Id).ToArray();
        
        return this with
        {
            ModifiedBy = @event.Author,
            ModifyById = @event.Author.Id,
            Tags = tags,
            TagsIds = tagIds
        };
    }

    public CandidateProjection Apply(CandidateTagRemoved @event)
    {
        var tags = Tags.Where(z => z.Id != @event.Tag.Id).ToArray();
        var tagIds = tags.Select(t => t.Id).ToArray();
        return this with
        {
            ModifiedBy = @event.RemovedBy,
            ModifyById = @event.RemovedBy.Id,
            Tags = tags,
            TagsIds = tagIds
        };
    }

    public CandidateProjection Apply(CandidateApplicationUpdated @event)
    {
        
        if (CompanyIds.Contains(@event.CompanyId)) return this;
        var companyIds = CompanyIds.Append(@event.CompanyId).ToArray();

        return this with
        {
            CompanyIds = companyIds
        };
    }
}