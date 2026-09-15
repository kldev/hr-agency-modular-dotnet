using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.SharedKernel.Tenant;

public interface IAudit
{
    UserSnapshot CreatedBy { get; }
    DateTimeOffset CreatedAt { get; }
    UserSnapshot? ModifiedBy { get; }
    DateTimeOffset? ModifiedAt { get; }
}

