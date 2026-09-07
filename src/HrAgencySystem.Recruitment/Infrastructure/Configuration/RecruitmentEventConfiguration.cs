using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.Recruitment.Events.JobPostings;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Configuration;

internal static class RecruitmentEventConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureRecruitmentEvents()
        {
            ConfigureJobApplicationEvents(options);
            ConfigureJobPostEvents(options);
            ConfigureCandidateEvents(options);
        }

        public void ConfigureRecruitmentEventsMinimal()
        {
            options.Events.AddEventType<JobApplicationCreated>();
            options.Events.AddEventType<CandidateCreated>();
            options.Events.AddEventType<CandidateApplicationUpdated>();
        }
    }

    private static void ConfigureJobApplicationEvents(StoreOptions options)
    {
        options.Events.AddEventType<JobApplicationCreated>();
        options.Events.AddEventType<JobApplicationAssessmentStarted>();
        options.Events.AddEventType<JobApplicationHired>();
        options.Events.AddEventType<JobApplicationInterviewScheduled>();
        options.Events.AddEventType<JobApplicationOfferMade>();
        options.Events.AddEventType<JobApplicationRejected>();
        options.Events.AddEventType<JobApplicationScreeningStarted>();
        options.Events.AddEventType<JobApplicationWithdrawn>();
        options.Events.AddEventType<JobApplicationTagged>();
        options.Events.AddEventType<JobApplicationTagRemoved>();
        options.Events.AddEventType<JobApplicationStatusChanged>();
        options.Events.AddEventType<JobApplicationNoteDeleted>();
    }

    private static void ConfigureJobPostEvents(StoreOptions options)
    {
        options.Events.AddEventType<JobPostCreated>();
        options.Events.AddEventType<JobPostUpdated>();
        options.Events.AddEventType<JobPostedToChannel>();
        options.Events.AddEventType<JobPostPublished>();
        options.Events.AddEventType<JobPostClosed>();
        options.Events.AddEventType<JobPostArchived>();
    }

    private static void ConfigureCandidateEvents(StoreOptions options)
    {
        options.Events.AddEventType<CandidateCreated>();
        options.Events.AddEventType<CandidateApplicationUpdated>();
        options.Events.AddEventType<CandidateTagged>();
        options.Events.AddEventType<CandidateTagRemoved>();
    }
}