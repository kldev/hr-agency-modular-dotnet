namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// The delivery a person was put on, as it stood when they were put on it.
/// <para>
/// A snapshot rather than an id, and the delivering company is the reason. An A1 is issued to a
/// named person by a named company for a named period; if this record only pointed at the project,
/// then renaming the company - or a project later carrying on under another one - would quietly
/// rewrite who posted somebody in 2024. The register has to keep saying what was true then.
/// </para>
/// <para>
/// The client and the work country come along for the same reason plus a practical one: the country
/// is half the key into the compliance catalogue, and reading it off a live project would make an
/// old assignment's requirements change under it.
/// </para>
/// </summary>
public sealed record ProjectPlacementSnapshot(
    Guid ProjectId,
    string ProjectName,
    Guid ClientCompanyId,
    string ClientCompanyName,
    Guid DeliveringEntityId,
    string DeliveringEntityName,
    string WorkCountry
);
