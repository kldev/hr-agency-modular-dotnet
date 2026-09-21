namespace HrAgencySystem.Agency.Domain;

/// <summary>
/// Somebody in a unit. <paramref name="Title"/> exists for the case the structure cannot express:
/// the second owner sits on the board with a title of their own while the other one is its head.
/// </summary>
public sealed record OrgUnitMember(Guid UserId, string Title);
