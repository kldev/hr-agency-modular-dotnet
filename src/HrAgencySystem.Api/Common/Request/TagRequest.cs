using System.ComponentModel;

namespace HrAgencySystem.Api.Common.Request;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record TagRequest(
    [property: Description("The tag to attach or remove - one of the agency's tags.")] Guid TagId
);

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record TagRequestList(
    [property: Description("The tags to attach or remove, in one call.")] IReadOnlyList<Guid> TagIds
);
