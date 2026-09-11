using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

internal static class InterviewProjectionExtensions
{
    internal static IQueryable<InterviewProjection> WithOrganizationId(this IQueryable<InterviewProjection> query,
        Guid organizationId)
    {
        return query.Where(i => i.OrgId == organizationId);
    }
    
    internal static IQueryable<InterviewProjection> WithInterviewId(this IQueryable<InterviewProjection> query,
        Guid interviewId)
    {
        return query.Where(i => i.Id == interviewId);
    }
    
    internal static IQueryable<InterviewProjection> WithJobApplicationId(this IQueryable<InterviewProjection> query,
        Guid? jobApplicationId)
    {
        return !jobApplicationId.HasValue || jobApplicationId.Value == Guid.Empty ? query : 
            query.Where(i => i.ApplicationId == jobApplicationId);
    }
    
    internal static IQueryable<InterviewProjection> WithCandidateId(this IQueryable<InterviewProjection> query,
        Guid? candidateId)
    {
        return !candidateId.HasValue || candidateId.Value == Guid.Empty ? query : 
            query.Where(i => i.CandidateId == candidateId);
    }
    
    internal static IQueryable<InterviewProjection> WithCreatedByUserId(this IQueryable<InterviewProjection> query,
        Guid? createdByUserId)
    {
        return !createdByUserId.HasValue || createdByUserId.Value == Guid.Empty ? query : 
            query.Where(i => i.CreatedByUserId == createdByUserId);
    }
    
    internal static IQueryable<InterviewProjection> WithInterviewerId(this IQueryable<InterviewProjection> query,
        Guid? interviewerId)
    {
        return !interviewerId.HasValue || interviewerId.Value == Guid.Empty ? query : 
            query.Where(i => i.InterviewerId == interviewerId);
    }

    internal static IQueryable<InterviewProjection> WithStatus(this IQueryable<InterviewProjection> query,
        InterviewStatus? status)
    {
        return !status.HasValue ? query : 
            query.Where(i => i.Status == status);
    }
    
    internal static IQueryable<InterviewProjection> WithScheduleFrom(this IQueryable<InterviewProjection> query,
        DateTimeOffset? from)
    {
        return !from.HasValue ? query : 
            query.Where(i => i.ScheduleAt >= from.Value);
    }
    
    internal static IQueryable<InterviewProjection> WithScheduleTo(this IQueryable<InterviewProjection> query,
        DateTimeOffset? to)
    {
        return !to.HasValue ? query : 
            query.Where(i => i.ScheduleAt < to.Value);
    }

    internal static IQueryable<InterviewProjection> WithSearch(this IQueryable<InterviewProjection> query,
        string search)
    {
        return string.IsNullOrWhiteSpace(search)
            ? query
            : query.Where(i => i.ApplicantInfo.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || i.ApplicantInfo.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || i.ApplicantInfo.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || i.ApplicantInfo.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
}