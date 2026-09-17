using HrAgencySystem.Recruitment.Application.Candidates.Queries;
using HrAgencySystem.Recruitment.Application.Interviews.Queries;
using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Application.JobApplications.Tags.Queries;
using HrAgencySystem.Recruitment.Application.JobPosting.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Application.Suggestion;
using HrAgencySystem.Recruitment.Infrastructure.Persistence;
using HrAgencySystem.Recruitment.Infrastructure.Query;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Port;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Recruitment.Infrastructure.Configuration;

public static class RecruitmentServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddRecruitmentServices(IConfiguration configuration)
        {
            services.AddScoped<ISeeder, TagSeeder>();

            services.AddScoped<IJobPostQueryRepository, JobPostQueryRepository>();
            services.AddScoped<ITagSuggestionRepository, TagSuggestionRepository>();
            services.AddScoped<ICandidateEmailReservationRepository, CandidateEmailReservationRepository>();
            services.AddScoped<ICandidateQueryRepository, CandidateQueryRepository>();
            services.AddScoped<ICandidateResolver, CandidateResolver>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IJobApplicationQueryRepository, JobApplicationQueryRepository>();

            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<INoteQueryRepository, NoteQueryRepository>();
            services.AddScoped<IInterviewsQueryRepository, InterviewsQueryRepository>();
            services.AddScoped<IJobApplicationInfoQueryRepository, JobApplicationInfoQueryRepository>();
            services.AddScoped<IRecruitmentService, RecruitmentService>();
            services.AddScoped<IJobPostSuggestionRepository, JobPostSuggestionRepository>();

            
        }
        
        public void AddRecruitmentServicesMinimal()
        {
            services.AddScoped<ICandidateResolver, CandidateResolver>();
            services.AddScoped<ICandidateEmailReservationRepository, CandidateEmailReservationRepository>();
            services.AddScoped<IJobPostQueryRepository, JobPostQueryRepository>();
        }
    }
}