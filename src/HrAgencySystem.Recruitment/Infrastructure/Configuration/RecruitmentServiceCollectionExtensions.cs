using HrAgencySystem.Recruitment.Application.Candidates.Queries;
using HrAgencySystem.Recruitment.Application.Interviews.Queries;
using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Application.JobApplications.Tags.Queries;
using HrAgencySystem.Recruitment.Application.JobPosting.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Application.Suggestion;
using HrAgencySystem.Recruitment.Config;
using HrAgencySystem.Recruitment.Feeds.Application.GenerateJobFeed;
using HrAgencySystem.Recruitment.Feeds.Application.GetJobFeed;
using HrAgencySystem.Recruitment.Feeds.Application.ScheduleFeedTasks;
using HrAgencySystem.Recruitment.Feeds.Persistence;
using HrAgencySystem.Recruitment.Feeds.Port;
using HrAgencySystem.Recruitment.Feeds.Worker;
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
            var section = configuration.GetSection(RecruitmentConfig.Section);

            services.Configure<RecruitmentConfig>(options =>
            {
                options.FeedUrl =
                    section[nameof(RecruitmentConfig.FeedUrl)] ?? string.Empty;
            });

            services.AddScoped<ISeeder, TagSeeder>();
            services.AddScoped<ISeeder, FeedMigration>();

            services.AddScoped<IJobPostQueryRepository, JobPostQueryRepository>();
            services.AddScoped<ITagSuggestionRepository, TagSuggestionRepository>();
            services.AddScoped<ICandidateEmailReservationRepository, CandidateEmailReservationRepository>();
            services.AddScoped<ICandidateQueryRepository, CandidateQueryRepository>();
            services.AddScoped<ICandidateResolver, CandidateResolver>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IJobApplicationQueryRepository, JobApplicationQueryRepository>();

            services.AddScoped<IJobFeedTaskRepository, JobFeedTaskRepository>();
            services.AddScoped<IJobFeedTaskQueue, JobFeedTaskQueue>();
            services.AddScoped<IJobFeedScheduler, JobFeedScheduler>();
            services.AddScoped<IJobFeedProcessor, JobFeedProcessor>();
            services.AddScoped<IJobFeedGenerator, JobFeedGenerator>();
            services.AddScoped<IJobFeedReader, JobFeedReader>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<INoteQueryRepository, NoteQueryRepository>();
            services.AddScoped<IInterviewsQueryRepository, InterviewsQueryRepository>();
            services.AddScoped<IJobApplicationInfoQueryRepository, JobApplicationInfoQueryRepository>();
            services.AddScoped<IRecruitmentService, RecruitmentService>();

            services.AddHostedService<JobFeedSchedulerWorker>();
            services.AddHostedService<JobFeedGenerationWorker>();
            
        }
        
        public void AddRecruitmentServicesMinimal()
        {
            services.AddScoped<ICandidateResolver, CandidateResolver>();
            services.AddScoped<ICandidateEmailReservationRepository, CandidateEmailReservationRepository>();
            services.AddScoped<IJobPostQueryRepository, JobPostQueryRepository>();
        }
    }
}