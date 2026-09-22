using System.ComponentModel;

namespace HrAgencySystem.Api.Common.Request;

internal sealed record ChangeResponsiblePersonRequest(
    [property: Description(
        "The user who takes over. Must belong to the caller's agency; handing it to somebody other than yourself sends them a mail."
    )]
        Guid ResponsibleId
);
