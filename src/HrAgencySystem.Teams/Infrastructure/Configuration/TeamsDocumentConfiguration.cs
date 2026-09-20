using HrAgencySystem.Teams.Infrastructure.Persistence;
using Marten;

namespace HrAgencySystem.Teams.Infrastructure.Configuration;

internal static class TeamsDocumentConfiguration
{
    private const string SchemaName = "teams";

    extension(StoreOptions options)
    {
        public void ConfigureDocuments()
        {
            // The unique index is the actual invariant: the handlers check first only so that the
            // ordinary case gets a readable message instead of a 409.
            options
                .Schema.For<TeamMembershipReservation>()
                .DatabaseSchemaName(SchemaName)
                .Index(
                    x => new { x.OrganizationId, x.UserId },
                    idx =>
                    {
                        idx.IsUnique = true;
                    }
                )
                .Index(x => new { x.OrganizationId, x.TeamId });
        }
    }
}
