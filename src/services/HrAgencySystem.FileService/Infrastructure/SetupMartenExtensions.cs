using HrAgencySystem.FileService.Domain;
using JasperFx;
using Marten;

namespace HrAgencySystem.FileService.Infrastructure;

public static class SetupMartenExtensions
{
    private const string SchemaName = "files";

    extension(IServiceCollection services)
    {
        public void SetupMartenForFileService(IConfiguration configuration)
        {
            services.AddMarten(options =>
            {
                options.Connection(
                    configuration.GetConnectionString("Postgres")
                        ?? throw new InvalidOperationException(
                            "ConnectionStrings:Postgres is required."
                        )
                );

                options.DatabaseSchemaName = SchemaName;

                options
                    .Schema.For<StoredFile>()
                    .DatabaseSchemaName(SchemaName)
                    .Index(x => new { x.OrganizationId })
                    .Index(x => new
                    {
                        x.OrganizationId,
                        x.OwnerKind,
                        x.OwnerId,
                    });

                options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
            });

            // No event store, no async daemon, no Wolverine: this service has no stream to project
            // and no message to send. Marten is here purely as a document store.
        }
    }
}
