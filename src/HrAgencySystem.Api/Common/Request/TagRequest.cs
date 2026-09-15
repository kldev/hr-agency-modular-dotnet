namespace HrAgencySystem.Api.Common.Request;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record TagRequest(Guid TagId);

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record TagRequestList(IReadOnlyList<Guid> TagIds);