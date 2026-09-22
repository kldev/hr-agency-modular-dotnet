using System.ComponentModel;

namespace HrAgencySystem.Api.Common.Request;

/// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record AssignRecruiterRequest(
    [property: Description(
        "The user to make responsible for recruiting. Must belong to the caller's agency."
    )]
        Guid RecruiterId
);
