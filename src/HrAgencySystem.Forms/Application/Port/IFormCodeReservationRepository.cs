namespace HrAgencySystem.Forms.Application.Port;

public interface IFormCodeReservationRepository
{
    Task<bool> ExistsAsync(Guid organizationId, string code, CancellationToken ct);

    /// <summary>Written in the handler's transaction; a concurrent twin fails on the unique index.</summary>
    void Reserve(Guid organizationId, string code, Guid formId);
}
