using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.SharedKernel.Snapshots;

public interface ICompanySnapshotRepository
{
    public const string NotFoundMessage = "Require company data not found.";
    Task<CompanySnapshot?> GetCompanyAsync(Guid companyId, CancellationToken ct);
}

/// <summary>
/// <para>
/// <paramref name="IsProfileComplete"/> and <paramref name="RegisteredAddress"/> default to "we do
/// not know": a caller that forgets to fill them in gets an incomplete profile and no address, so
/// the rules that depend on them refuse rather than wave something through.
/// </para>
/// </summary>
public sealed record CompanySnapshot(
    Guid Id,
    string Name,
    string TaxId,
    bool IsProfileComplete = false,
    PostalAddress? RegisteredAddress = null,
    string? LegalName = null,
    string? VatNumber = null
)
{
    /// <summary>The name to put on a contract: the registered one when we have it.</summary>
    public string ContractName => string.IsNullOrWhiteSpace(LegalName) ? Name : LegalName;
}
