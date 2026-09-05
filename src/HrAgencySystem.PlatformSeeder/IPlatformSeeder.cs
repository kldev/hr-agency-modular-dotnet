namespace HrAgencySystem.PlatformSeeder;

public interface IPlatformSeeder
{
    Task Seed();
    Task SeedApplicants(int count);
    Task SeedShowcase();
}