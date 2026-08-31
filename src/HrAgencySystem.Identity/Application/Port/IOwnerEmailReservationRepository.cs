using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Identity.Application.Port;

public interface IOwnerEmailReservationRepository
{
    Task<bool> ExistAsync(Email email, CancellationToken ct);
    Task ReserveAsync(Email email);
}