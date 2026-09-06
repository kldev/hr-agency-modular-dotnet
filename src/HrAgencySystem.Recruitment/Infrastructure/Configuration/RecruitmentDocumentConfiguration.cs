using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.Recruitment.Infrastructure.Persistence;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Configuration;

internal static class RecruitmentDocumentConfiguration
{
    private const string SchemaName = "recruitment";

    extension(StoreOptions options)
    {
        public void ConfigureRecruitmentDocuments()
        {
            ConfigureTag(options);
            ConfigureJobApplicationNote(options);
            ConfigureCandidateEmailReservation(options);
        }

        public void ConfigureRecruitmentDocumentsMinimal()
        {
            ConfigureCandidateEmailReservation(options);
        }
    }

    private static void ConfigureTag(StoreOptions options)
    {
        options.Schema
            .For<Tag>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.Category)
            .Index(
                x => new { x.Category, x.Code },
                idx => idx.IsUnique = true)
            .Index(x => x.Name);
    }

    private static void ConfigureJobApplicationNote(StoreOptions options)
    {
        options.Schema
            .For<JobApplicationNote>()
            .DatabaseSchemaName(SchemaName)
            .Index(
                x => new { x.Id, x.OrgId },
                idx => idx.IsUnique = true)
            .Index(x => new { x.OrgId, x.JobApplicationId });
    }

    private static void ConfigureCandidateEmailReservation(StoreOptions options)
    {
        options.Schema
            .For<CandidateEmailReservation>()
            .DatabaseSchemaName(SchemaName)
            .Index(
                x => new { x.Email, x.OrganizationId },
                idx => idx.IsUnique = true);
    }
}