using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// Turns what the legal entities module knows into what a project needs to keep. One place, so the
/// two callers that record a delivering entity - creating a project and swapping it on a draft -
/// cannot copy a different subset of the fields.
/// </summary>
public static class DeliveringEntity
{
    public static DeliveringEntitySnapshot From(LegalEntitySnapshot entity)
    {
        return new DeliveringEntitySnapshot(
            entity.Id,
            entity.Name,
            entity.LegalName,
            entity.TaxId,
            entity.VatNumber,
            entity.RegisteredAddress
        );
    }
}
