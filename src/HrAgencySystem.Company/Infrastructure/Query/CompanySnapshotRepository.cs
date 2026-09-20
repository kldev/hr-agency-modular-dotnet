using HrAgencySystem.Company.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Query;

/// <summary>
/// Reads the projection first and falls back to replaying the stream.
/// <para>
/// The fallback is not belt and braces: the projection runs in the async daemon, and the question
/// this port is asked - may we put this client on a contract - is asked in the seconds right after
/// somebody filled the profile in. Answering "no" because a read model has not caught up yet would
/// refuse a perfectly good contract and leave nobody able to explain why.
/// </para>
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class CompanySnapshotRepository(IDocumentSession session) : ICompanySnapshotRepository
{
    public async Task<CompanySnapshot?> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        var projection = await session
            .Query<CompanyProjection>()
            .WithCompanyId(companyId)
            .FirstOrDefaultAsync(ct);

        if (projection is not null)
            return Describe(projection);

        var aggregate = await session.Events.AggregateStreamAsync<Domain.Company>(
            companyId,
            token: ct
        );

        return aggregate is null ? null : Describe(aggregate);
    }

    public async Task<CompanySnapshot?> GetCompanyAsync(
        Guid companyId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var projection = await session
            .Query<CompanyProjection>()
            .WithCompanyId(companyId)
            .FirstOrDefaultAsync(ct);

        if (projection is not null)
            return projection.OrganizationId == organizationId.Value ? Describe(projection) : null;

        var aggregate = await session.Events.AggregateStreamAsync<Domain.Company>(
            companyId,
            token: ct
        );

        if (aggregate is null || aggregate.OrganizationId.Value != organizationId.Value)
            return null;

        return Describe(aggregate);
    }

    private static CompanySnapshot Describe(CompanyProjection projection) =>
        new(
            projection.Id,
            projection.Name,
            projection.TaxId,
            projection.IsProfileComplete,
            projection.Profile.RegisteredAddress,
            projection.Profile.LegalName,
            projection.Profile.VatNumber
        );

    private static CompanySnapshot Describe(Domain.Company company) =>
        new(
            company.Id.Value,
            company.Name.Value,
            company.TaxId?.Value ?? "",
            company.IsProfileComplete,
            company.Profile.RegisteredAddress,
            company.Profile.LegalName,
            company.Profile.VatNumber
        );
}
