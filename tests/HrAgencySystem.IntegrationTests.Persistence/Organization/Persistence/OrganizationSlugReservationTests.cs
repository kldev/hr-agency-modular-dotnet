using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Organization;
using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Organization.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Tenant;
using JasperFx;
using Marten;
using Npgsql;

namespace HrAgencySystem.IntegrationTests.Organization.Persistence;

[Collection(PostgresCollection.Name)]
public sealed class OrganizationSlugReservationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private const string ReservedSlug = "hr-agency";
    private const string OtherReservedSlug = "flex-jobs";
    private const string FreeSlug = "abc-work";

    private IDocumentStore _store = null!;

    public async Task InitializeAsync()
    {
        var storeOptions = new StoreOptions();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(fixture.ConnectionString);

        storeOptions.Connection(dataSourceBuilder.Build());
        storeOptions.Events.DatabaseSchemaName = "events";
        storeOptions.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

        OrganizationModule.ConfigureMarten(storeOptions);

        _store = new DocumentStore(storeOptions);

        var cleaner = new DatabaseCleaner(fixture.ConnectionString);

        await cleaner.CleanOrganizationReservation();
    }

    public async Task DisposeAsync()
    {
        await _store.DisposeAsync();
    }

    /// <summary>
    /// Every call gets its own session on purpose - a reservation has to survive the session that
    /// wrote it, and concurrent requests never share one.
    /// </summary>
    private async Task ReserveInOwnSession(OrganizationId organizationId, string slug)
    {
        await using var session = _store.LightweightSession();

        var repository = new OrganizationSlugReservationRepository(session);

        await repository.Reserve(organizationId, OrganizationSlug.Create(slug));

        await session.SaveChangesAsync(CancellationToken.None);
    }

    private async Task<OrganizationId?> FindBySlug(string slug)
    {
        await using var session = _store.LightweightSession();

        var repository = new OrganizationSlugReservationRepository(session);

        return await repository.FindBySlug(OrganizationSlug.Create(slug), CancellationToken.None);
    }

    private async Task<bool> Exists(string slug)
    {
        await using var session = _store.LightweightSession();

        var repository = new OrganizationSlugReservationRepository(session);

        return await repository.Exists(OrganizationSlug.Create(slug), CancellationToken.None);
    }

    private async Task<int> CountReservations()
    {
        await using var session = _store.QuerySession();

        return await session
            .Query<OrganizationSlugReservation>()
            .CountAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Reserve_ShouldStoreReservation_WithGivenSlugAndOrganization()
    {
        // Arrange
        var organizationId = OrganizationId.NewId();

        // Act
        await ReserveInOwnSession(organizationId, ReservedSlug);

        // Assert
        var stored = await FindBySlug(ReservedSlug);

        Assert.NotNull(stored);
        Assert.Equal(organizationId, stored.Value);
        Assert.Equal(1, await CountReservations());
    }

    [Fact]
    public async Task Reserve_ShouldAllowManySlugs_ForTheSameOrganization()
    {
        // Arrange - an organization keeps its old slugs after a rename
        var organizationId = OrganizationId.NewId();

        // Act
        await ReserveInOwnSession(organizationId, ReservedSlug);
        await ReserveInOwnSession(organizationId, OtherReservedSlug);

        // Assert
        Assert.Equal(organizationId, await FindBySlug(ReservedSlug));
        Assert.Equal(organizationId, await FindBySlug(OtherReservedSlug));
    }

    [Fact]
    public async Task Reserve_ShouldThrow_WhenSlugIsAlreadyReservedByAnotherOrganization()
    {
        // Arrange
        var owner = OrganizationId.NewId();

        await ReserveInOwnSession(owner, ReservedSlug);

        // Act - a separate session, so the unique index is what rejects the insert,
        // not Marten's tracking of the first one
        await Assert.ThrowsAsync<DocumentAlreadyExistsException>(() =>
            ReserveInOwnSession(OrganizationId.NewId(), ReservedSlug)
        );

        // Assert
        Assert.Equal(1, await CountReservations());
        Assert.Equal(owner, await FindBySlug(ReservedSlug));
    }

    [Fact]
    public async Task Reserve_ShouldThrow_WhenSlugIsAlreadyReservedByTheSameOrganization()
    {
        // Arrange
        var organizationId = OrganizationId.NewId();

        await ReserveInOwnSession(organizationId, ReservedSlug);

        // Act
        await Assert.ThrowsAsync<DocumentAlreadyExistsException>(() =>
            ReserveInOwnSession(organizationId, ReservedSlug)
        );

        // Assert
        Assert.Equal(1, await CountReservations());
    }

    [Fact]
    public async Task FindBySlug_ShouldReturnOwner_WhenSlugIsReserved()
    {
        // Arrange
        var owner = OrganizationId.NewId();

        await ReserveInOwnSession(OrganizationId.NewId(), OtherReservedSlug);
        await ReserveInOwnSession(owner, ReservedSlug);

        // Act
        var result = await FindBySlug(ReservedSlug);

        // Assert
        Assert.Equal(owner, result);
    }

    [Fact]
    public async Task FindBySlug_ShouldReturnNull_WhenSlugIsNotReserved()
    {
        // Arrange
        await ReserveInOwnSession(OrganizationId.NewId(), ReservedSlug);

        // Act
        var result = await FindBySlug(FreeSlug);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Exists_ShouldReturnTrue_WhenSlugIsReserved()
    {
        // Arrange
        await ReserveInOwnSession(OrganizationId.NewId(), ReservedSlug);
        await ReserveInOwnSession(OrganizationId.NewId(), OtherReservedSlug);

        // Act
        var result = await Exists(OtherReservedSlug);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Exists_ShouldReturnFalse_WhenSlugIsNotReserved()
    {
        // Arrange
        await ReserveInOwnSession(OrganizationId.NewId(), ReservedSlug);
        await ReserveInOwnSession(OrganizationId.NewId(), OtherReservedSlug);

        // Act
        var result = await Exists(FreeSlug);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Exists_ShouldReturnTrue_WhenSlugDiffersOnlyByCaseOrWhitespace()
    {
        // Arrange - OrganizationSlug trims and lowercases, so both forms hit the same reservation
        await ReserveInOwnSession(OrganizationId.NewId(), $"  {ReservedSlug.ToUpperInvariant()} ");

        // Act
        var result = await Exists(ReservedSlug);

        // Assert
        Assert.True(result);
    }
}
