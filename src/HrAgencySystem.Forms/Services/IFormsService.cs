using HrAgencySystem.Forms.Domain;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Forms.Services;

/// <summary>
/// The module's one way out. What a form needs from elsewhere is who is acting and who the form is
/// about; both come through SharedKernel ports, never through a reference to another module.
/// </summary>
public interface IFormsService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);

    Task ValidateOrganization(Guid organizationId, CancellationToken ct);

    /// <summary>
    /// A 404 for somebody else's record, never a 403 - a different answer would confirm the id
    /// exists in another organization.
    /// </summary>
    void ValidateAggregateUpdate(IOrganizationDomain? aggregate, Guid commandOrganizationId, string name, Guid id);

    /// <summary>
    /// The person a response is about, within this organization. The one place the module admits
    /// it knows what a worker is (see <see cref="SubjectKinds"/>). Throws a 404 when there is no
    /// such subject here.
    /// </summary>
    Task<SubjectSnapshot> GetSubjectAsync(OrganizationId organizationId, SubjectRef subject, CancellationToken ct);
}

/// <summary>
/// A subject, in the few words forms need: a name to show, and the worker's file when that is what
/// it is - the pre-fill source for system fields.
/// </summary>
public sealed record SubjectSnapshot(SubjectRef Subject, string DisplayName, WorkerSnapshot? Worker);
