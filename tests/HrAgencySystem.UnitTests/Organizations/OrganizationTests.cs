using HrAgencySystem.Organization.Events;
using D = HrAgencySystem.Organization.Domain;
namespace HrAgencySystem.UnitTests.Organizations;

public class OrganizationTests
{
    [Fact]
    public void Apply_organization_created_event_creates_organization_state()
    {
        var organizationId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(
            2026, 8, 30, 12, 0, 0, TimeSpan.Zero);

        var @event = new OrganizationCreated(
            organizationId,
            "Acme Sp. z o.o.",
            "acme",
            createdAt);


        var organization = D.Organization.Empty();

        organization.Apply(@event);

        Assert.Equal(
            organizationId,
            organization.Id.Value);

        Assert.Equal(
            "Acme Sp. z o.o.",
            organization.Name.Value);

        Assert.Equal(
            "acme",
            organization.Slug.Value);

        Assert.Equal(
            createdAt,
            organization.CreatedAt);
    }
}