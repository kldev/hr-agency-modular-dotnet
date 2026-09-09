namespace HrAgencySystem.PlatformSeeder;

public interface IPlatformSalesSeeder
{
    Task Seed(int opportunityCount = 500, string slug = "hr-agency", CancellationToken ct = default);
}