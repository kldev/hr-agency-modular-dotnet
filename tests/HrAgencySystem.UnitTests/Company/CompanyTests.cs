using HrAgencySystem.Company.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using D = HrAgencySystem.Company.Domain;

namespace HrAgencySystem.UnitTests.Company;

public class CompanyTests
{
    [Fact]
    public void Apply_company_created_event_creates_company_state()
    {
        var companyId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(
            2026, 8, 30, 12, 0, 0, TimeSpan.Zero);

        var user = new UserSnapshot(Guid.NewGuid(), "Test", "Tester", "test@test.io");
        
        var @event = new CompanyCreated(
            companyId,
            organizationId,
            "Acme Sp. z o.o.",
            "PL",
            "PL123456789",
            "KRS-123456",
            user,
            createdAt);

        var company = D.Company.Empty();
        company.Apply(@event);

        Assert.Equal(companyId, company.Id.Value);
        Assert.Equal(organizationId, company.OrganizationId.Value);
        Assert.Equal("Acme Sp. z o.o.", company.Name.Value);
        Assert.Equal("PL", company.CountryCode.Value);
        Assert.Equal("PL123456789", company.TaxId!.Value);
        Assert.Equal("KRS-123456", company.RegistrationNumber!.Value);
        Assert.Equal(D.CompanyStatus.Active, company.Status);
        Assert.Equal(createdAt, company.CreatedAt);
    }
}