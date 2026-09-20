using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Projections;

namespace HrAgencySystem.Workers.Application.Port;

public interface IWorkersQueryRepository
{
    Task<SliceResponse<WorkerProjection>> GetWorkers(
        OrganizationId organizationId,
        WorkerQuery query,
        CancellationToken ct
    );

    Task<WorkerProjection?> GetWorker(
        OrganizationId organizationId,
        Guid workerId,
        CancellationToken ct
    );

    /// <summary>
    /// Somebody who looks like this person already being on the books. One file per human being is
    /// the whole point of the register: a second one splits their postings in half, and then
    /// neither half can be asked whether they hold a valid A1.
    /// <para>
    /// Two ways of recognising them, because neither works alone: the e-mail address, and the name
    /// together with the phone number for the many people who have no work address. A name on its
    /// own is not enough - two people really are called Jan Kowalski.
    /// </para>
    /// <para>
    /// Read from the projection, so a second file created in the same second can still get in. The
    /// e-mail reservation is what closes that door for the case it can; the rest is a check meant
    /// to be read by a person who then goes and adds an assignment instead.
    /// </para>
    /// </summary>
    Task<WorkerProjection?> FindDuplicate(
        OrganizationId organizationId,
        string? email,
        string firstName,
        string lastName,
        string phoneNumber,
        Guid? exceptWorkerId,
        CancellationToken ct
    );
}

/// <summary>
/// <para>
/// <paramref name="WorkCountries"/> and <paramref name="ExcludeWorkCountries"/> are what the two
/// halves of the office filter on. Whoever looks after people working in Poland and whoever looks
/// after the ones abroad want the same screen over two different populations, and the split runs
/// along the country of the delivery somebody is on.
/// </para>
/// <para>
/// Which country counts as home is deliberately not decided here. The backend takes a list to
/// include and a list to leave out; calling one of them "domestic" would bake an agency's own
/// address into a product that has tenants in more than one country.
/// </para>
/// </summary>
public sealed record WorkerQuery(
    string Search,
    IReadOnlyList<WorkerStatus>? Statuses,
    IReadOnlyList<ResponsibleDepartment>? Departments,
    IReadOnlyList<string>? WorkCountries,
    IReadOnlyList<string>? ExcludeWorkCountries,
    string? Citizenship,
    int Page,
    int PageSize
) : IPagedQuery;
