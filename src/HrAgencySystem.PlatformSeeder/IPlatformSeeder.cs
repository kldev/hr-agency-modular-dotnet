namespace HrAgencySystem.PlatformSeeder;

public interface IPlatformSeeder
{
    Task Seed();
    Task SeedApplicants(int count);
    Task SeedShowcase();

    /// <summary>
    /// Legal entities, projects, workers and assignments for an agency that already exists. Split
    /// out from <see cref="Seed"/> because it is the part worth re-running on its own: the rest of
    /// the showcase takes minutes and nothing in the register depends on it beyond users and
    /// clients, which are already there.
    /// </summary>
    Task SeedDelivery(string slug);
}
