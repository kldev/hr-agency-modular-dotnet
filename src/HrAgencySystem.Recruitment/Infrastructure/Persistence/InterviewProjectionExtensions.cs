using HrAgencySystem.Recruitment.Application.Interviews.Queries;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

internal static class InterviewProjectionExtensions
{
    internal static IQueryable<InterviewProjection> WithQuery(
        this IQueryable<InterviewProjection> query,
        Guid organizationId,
        InterviewsQuery filter
    )
    {
        return query
            .WithOrganizationId(organizationId)
            .WithInterviewerId(filter.InterviewerId)
            .WithCandidateId(filter.CandidateId)
            .WithCreatedByUserId(filter.CreatedByUserId)
            .WithJobApplicationId(filter.JobApplicationId)
            .WithStatus(filter.Status)
            .WithScheduleFrom(filter.From)
            .WithScheduleTo(filter.To)
            .WithSearch(filter.Search);
    }

    internal static IQueryable<InterviewProjection> WithOrganizationId(
        this IQueryable<InterviewProjection> query,
        Guid organizationId
    )
    {
        return query.Where(i => i.OrgId == organizationId);
    }

    internal static IQueryable<InterviewProjection> WithInterviewId(
        this IQueryable<InterviewProjection> query,
        Guid interviewId
    )
    {
        return query.Where(i => i.Id == interviewId);
    }

    private static IQueryable<InterviewProjection> WithJobApplicationId(
        this IQueryable<InterviewProjection> query,
        Guid? jobApplicationId
    )
    {
        return !jobApplicationId.HasValue || jobApplicationId.Value == Guid.Empty
            ? query
            : query.Where(i => i.ApplicationId == jobApplicationId);
    }

    private static IQueryable<InterviewProjection> WithCandidateId(
        this IQueryable<InterviewProjection> query,
        Guid? candidateId
    )
    {
        return !candidateId.HasValue || candidateId.Value == Guid.Empty
            ? query
            : query.Where(i => i.CandidateId == candidateId);
    }

    private static IQueryable<InterviewProjection> WithCreatedByUserId(
        this IQueryable<InterviewProjection> query,
        Guid? createdByUserId
    )
    {
        return !createdByUserId.HasValue || createdByUserId.Value == Guid.Empty
            ? query
            : query.Where(i => i.CreatedByUserId == createdByUserId);
    }

    private static IQueryable<InterviewProjection> WithInterviewerId(
        this IQueryable<InterviewProjection> query,
        Guid? interviewerId
    )
    {
        return !interviewerId.HasValue || interviewerId.Value == Guid.Empty
            ? query
            : query.Where(i => i.InterviewerId == interviewerId);
    }

    private static IQueryable<InterviewProjection> WithStatus(
        this IQueryable<InterviewProjection> query,
        InterviewStatus? status
    )
    {
        return !status.HasValue ? query : query.Where(i => i.Status == status);
    }

    private static IQueryable<InterviewProjection> WithScheduleFrom(
        this IQueryable<InterviewProjection> query,
        DateTimeOffset? from
    )
    {
        return !from.HasValue ? query : query.Where(i => i.ScheduleAt >= from.Value);
    }

    private static IQueryable<InterviewProjection> WithScheduleTo(
        this IQueryable<InterviewProjection> query,
        DateTimeOffset? to
    )
    {
        return !to.HasValue ? query : query.Where(i => i.ScheduleAt < to.Value);
    }

    private static IQueryable<InterviewProjection> WithSearch(
        this IQueryable<InterviewProjection> query,
        string search
    )
    {
        return string.IsNullOrWhiteSpace(search)
            ? query
            : query.Where(i =>
                i.ApplicantInfo.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                || i.ApplicantInfo.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || i.ApplicantInfo.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || i.ApplicantInfo.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || i.ApplicantInfo.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
            );
    }
}
