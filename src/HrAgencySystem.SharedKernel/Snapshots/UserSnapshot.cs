namespace HrAgencySystem.SharedKernel.Snapshots;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record UserSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);