using HrAgencySystem.Company.Events;
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
        var owner = await FindOwnerAsync(companyId, ct);

        // Known to belong elsewhere: the only honest answer is "no such company here".
        if (owner is not null && owner != organizationId.Value)
            return null;

        return await FindAsync(companyId, ct) ?? Invent(companyId);
    }

    private async Task<CompanySnapshot?> FindAsync(Guid companyId, CancellationToken ct)
    {
        var projection = await session
            .Query<CompanyProjection>()
            .Where(z => z.Id == companyId)
            .FirstOrDefaultAsync(ct);

        if (projection is not null)
            return new CompanySnapshot(
                projection.Id,
                projection.Name,
                projection.TaxId,
                projection.IsProfileComplete,
                projection.Profile.RegisteredAddress,
                projection.Profile.LegalName,
                projection.Profile.VatNumber
            );

        // The projection runs in the async daemon; the creation event is written inline, so it is
        // the only thing available in the moment right after a company is created.
        var created = await session
            .Query<CompanyCreated>()
            .Where(z => z.CompanyId == companyId)
            .FirstOrDefaultAsync(ct);

        return created is null
            ? null
            : new CompanySnapshot(created.CompanyId, created.Name, created.TaxId);
    }

    private async Task<Guid?> FindOwnerAsync(Guid companyId, CancellationToken ct)
    {
        var projection = await session
            .Query<CompanyProjection>()
            .Where(z => z.Id == companyId)
            .Select(z => (Guid?)z.OrganizationId)
            .FirstOrDefaultAsync(ct);

        if (projection is not null)
            return projection;

        return await session
            .Query<CompanyCreated>()
            .Where(z => z.CompanyId == companyId)
            .Select(z => (Guid?)z.OrganizationId)
            .FirstOrDefaultAsync(ct);
    }

    private static CompanySnapshot Invent(Guid companyId)
    {
        var suffix = companyId.ToString()[4..];

        return new CompanySnapshot(companyId, "Company  " + suffix, "TXT 101-200" + suffix);
    }
}
