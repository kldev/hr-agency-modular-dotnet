using HrAgencySystem.Company.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

/// <summary>
/// Answers from the real company data when the test actually created a company, and invents one
/// otherwise. The invention is what lets a test about job posts use a random company id without
/// standing up the company module; the real lookup is what lets a test about projects assert on the
/// profile and on tenant isolation, which an invented company could never fail.
/// <para>
/// Like the production repository, it falls back to replaying the stream when the projection has not
/// caught up - otherwise every test would have to wait for the daemon before it could do anything
/// with the company it just created.
/// </para>
/// </summary>
public sealed class FakeCompanySnapshot(IQuerySession session) : ICompanySnapshotRepository
{
    public async Task<CompanySnapshot?> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        return await FindAsync(companyId, ct) ?? Invent(companyId);
    }

    public async Task<CompanySnapshot?> GetCompanyAsync(
        Guid companyId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var (snapshot, owner) = await FindWithOwnerAsync(companyId, ct);

        if (owner is null)
            return Invent(companyId);

        // Known to belong elsewhere: the only honest answer is "no such company here".
        return owner == organizationId.Value ? snapshot : null;
    }

    private async Task<CompanySnapshot?> FindAsync(Guid companyId, CancellationToken ct) =>
        (await FindWithOwnerAsync(companyId, ct)).Snapshot;

    private async Task<(CompanySnapshot? Snapshot, Guid? Owner)> FindWithOwnerAsync(
        Guid companyId,
        CancellationToken ct
    )
    {
        var projection = await session
            .Query<CompanyProjection>()
            .Where(z => z.Id == companyId)
            .FirstOrDefaultAsync(ct);

        if (projection is not null)
            return (
                new CompanySnapshot(
                    projection.Id,
                    projection.Name,
                    projection.TaxId,
                    projection.IsProfileComplete,
                    projection.Profile.RegisteredAddress,
                    projection.Profile.LegalName,
                    projection.Profile.VatNumber
                ),
                projection.OrganizationId
            );

        var company = await session.Events.AggregateStreamAsync<HrAgencySystem.Company.Domain.Company>(
            companyId,
            token: ct
        );

        if (company is null)
            return (null, null);

        return (
            new CompanySnapshot(
                company.Id.Value,
                company.Name.Value,
                company.TaxId?.Value ?? "",
                company.IsProfileComplete,
                company.Profile.RegisteredAddress,
                company.Profile.LegalName,
                company.Profile.VatNumber
            ),
            company.OrganizationId.Value
        );
    }

    private static CompanySnapshot Invent(Guid companyId)
    {
        var suffix = companyId.ToString()[4..];

        return new CompanySnapshot(companyId, "Company  " + suffix, "TXT 101-200" + suffix);
    }
}
