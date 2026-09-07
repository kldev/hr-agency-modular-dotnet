using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Application.Interviews.Queries;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Infrastructure.Persistence;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using Marten;
using Npgsql;

namespace HrAgencySystem.IntegrationTests.Interviews;

[Collection(PostgresCollection.Name)]
public class InterviewsQueryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private IDocumentStore _store = null!;
    private IQuerySession _session = null!;
    private InterviewsQueryRepository _repository = null!;


    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _otherOrganizationId = Guid.NewGuid();

    private readonly UserSnapshot _user =
        new(
            Guid.NewGuid(),
            "John",
            "Smith",
            "j-smith@phpdemo.com");

    public async Task InitializeAsync()
    {
        var storeOptions = new StoreOptions();

        var dataSourceBuilder =
            new NpgsqlDataSourceBuilder(fixture.ConnectionString);

        storeOptions.Connection(dataSourceBuilder.Build());
        storeOptions.RegisterDocumentType<InterviewProjection>();
        
        storeOptions.DatabaseSchemaName = "recruitment";
        
        _store = new DocumentStore(storeOptions);
        _session = _store.QuerySession();
        _repository = new InterviewsQueryRepository(_session);

        var cleaner = new DatabaseCleaner(fixture.ConnectionString);

        await cleaner.CleanInterviews();


    }

    public async Task DisposeAsync()
    {
        _session.Dispose();
        await _store.DisposeAsync();
    }

    [Fact]
    public async Task Get_ShouldReturnInterview_WhenInterviewBelongsToOrganization()
    {
        // Arrange
        var interview = CreateInterview();

        await Store(interview);

        // Act
        var result = await _repository.Get(
            _organizationId,
            interview.Id,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(interview.Id, result.Id);
    }

    [Fact]
    public async Task Get_ShouldReturnNull_WhenInterviewBelongsToAnotherOrganization()
    {
        // Arrange
        var interview = CreateInterview(
            organizationId: _otherOrganizationId);

        await Store(interview);

        // Act
        var result = await _repository.Get(
            _organizationId,
            interview.Id,
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Get_ShouldReturnNull_WhenInterviewDoesNotExist()
    {
        // Act
        var result = await _repository.Get(
            _organizationId,
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSlice_ShouldReturnOnlyInterviewsFromOrganization()
    {
        // Arrange
        var expected = CreateInterview();

        var otherOrganizationInterview = CreateInterview(
            organizationId: _otherOrganizationId);

        await Store(
            expected,
            otherOrganizationInterview);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(),
            CancellationToken.None);

        // Assert
        Assert.Single(result.Content);
        Assert.Equal(expected.Id, result.Content[0].Id);
    }

    [Fact]
    public async Task GetSlice_ShouldFilterByInterviewerId()
    {
        // Arrange
        var interviewerId = Guid.NewGuid();

        var expected = CreateInterview(
            interviewerId: interviewerId);

        var other = CreateInterview(
            interviewerId: Guid.NewGuid());

        await Store(expected, other);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(interviewerId: interviewerId),
            CancellationToken.None);

        // Assert
        Assert.Single(result.Content);
        Assert.Equal(expected.Id, result.Content[0].Id);
    }

    [Fact]
    public async Task GetSlice_ShouldFilterByCandidateId()
    {
        // Arrange
        var candidateId = Guid.NewGuid();

        var expected = CreateInterview(
            candidateId: candidateId);

        var other = CreateInterview(
            candidateId: Guid.NewGuid());

        await Store(expected, other);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(candidateId: candidateId),
            CancellationToken.None);

        // Assert
        Assert.Single(result.Content);
        Assert.Equal(expected.Id, result.Content[0].Id);
    }

    [Fact]
    public async Task GetSlice_ShouldFilterByJobApplicationId()
    {
        // Arrange
        var jobApplicationId = Guid.NewGuid();

        var expected = CreateInterview(
            applicationId: jobApplicationId);

        var other = CreateInterview(
            applicationId: Guid.NewGuid());

        await Store(expected, other);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(jobApplicationId: jobApplicationId),
            CancellationToken.None);

        // Assert
        Assert.Single(result.Content);
        Assert.Equal(expected.Id, result.Content[0].Id);
    }

    [Fact]
    public async Task GetSlice_ShouldFilterByCreatedByUserId()
    {
        // Arrange
        var createdByUserId = Guid.NewGuid();

        var expected = CreateInterview(
            createdByUserId: createdByUserId);

        var other = CreateInterview(
            createdByUserId: Guid.NewGuid());

        await Store(expected, other);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(createdByUserId: createdByUserId),
            CancellationToken.None);

        // Assert
        Assert.Single(result.Content);
        Assert.Equal(expected.Id, result.Content[0].Id);
    }

    [Fact]
    public async Task GetSlice_ShouldFilterByStatus()
    {
        // Arrange
        var expected = CreateInterview(
            status: InterviewStatus.Planned);

        var other = CreateInterview(
            status: InterviewStatus.Completed);

        await Store(expected, other);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(status: InterviewStatus.Planned),
            CancellationToken.None);

        // Assert
        Assert.Single(result.Content);
        Assert.Equal(expected.Id, result.Content[0].Id);
    }

    [Fact]
    public async Task GetSlice_ShouldFilterByScheduleFrom()
    {
        // Arrange
        var from = new DateTimeOffset(
            2026,
            9,
            10,
            10,
            0,
            0,
            TimeSpan.Zero);

        var before = CreateInterview(
            scheduleAt: from.AddSeconds(-1));

        var expected = CreateInterview(
            scheduleAt: from);

        var after = CreateInterview(
            scheduleAt: from.AddHours(1));

        await Store(
            before,
            expected,
            after);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(from: from),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Content.Count);

        Assert.Equal(after.Id, result.Content[0].Id);
        Assert.Equal(expected.Id, result.Content[1].Id);
    }

    [Fact]
    public async Task GetSlice_ShouldFilterByScheduleTo()
    {
        // Arrange
        var to = new DateTimeOffset(
            2026,
            9,
            11,
            0,
            0,
            0,
            TimeSpan.Zero);

        var before = CreateInterview(
            scheduleAt: to.AddHours(-2));

        var expected = CreateInterview(
            scheduleAt: to.AddSeconds(-1));

        var after = CreateInterview(
            scheduleAt: to);

        await Store(
            before,
            expected,
            after);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(to: to),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Content.Count);

        Assert.Equal(expected.Id, result.Content[0].Id);
        Assert.Equal(before.Id, result.Content[1].Id);
    }
    [Fact]
    public async Task GetSlice_ShouldFilterByScheduleDateRange()
    {
        // Arrange
        var from = new DateTimeOffset(
            2026,
            9,
            10,
            0,
            0,
            0,
            TimeSpan.Zero);

        var to = new DateTimeOffset(
            2026,
            9,
            10,
            23,
            59,
            59,
            TimeSpan.Zero);

        var before = CreateInterview(
            scheduleAt: from.AddSeconds(-1));

        var morning = CreateInterview(
            scheduleAt: from.AddHours(8));

        var afternoon = CreateInterview(
            scheduleAt: from.AddHours(14));

        var after = CreateInterview(
            scheduleAt: to.AddSeconds(1));

        await Store(
            before,
            morning,
            afternoon,
            after);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(
                from: from,
                to: to),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Content.Count);

        Assert.Equal(afternoon.Id, result.Content[0].Id);
        Assert.Equal(morning.Id, result.Content[1].Id);
    }

    [Fact]
    public async Task GetSlice_ShouldOrderByScheduleDescending()
    {
        // Arrange
        var oldest = CreateInterview(
            scheduleAt: new DateTimeOffset(
                2026,
                9,
                10,
                9,
                0,
                0,
                TimeSpan.Zero));

        var middle = CreateInterview(
            scheduleAt: new DateTimeOffset(
                2026,
                9,
                10,
                12,
                0,
                0,
                TimeSpan.Zero));

        var newest = CreateInterview(
            scheduleAt: new DateTimeOffset(
                2026,
                9,
                10,
                15,
                0,
                0,
                TimeSpan.Zero));

        await Store(
            oldest,
            middle,
            newest);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(),
            CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Content.Count);

        Assert.Equal(newest.Id, result.Content[0].Id);
        Assert.Equal(middle.Id, result.Content[1].Id);
        Assert.Equal(oldest.Id, result.Content[2].Id);
    }

    [Fact]
    public async Task GetSlice_ShouldReturnAllInterviews_WhenNoFiltersAreSpecified()
    {
        // Arrange
        var first = CreateInterview();
        var second = CreateInterview();
        var third = CreateInterview();

        await Store(first, second, third);

        // Act
        var result = await _repository.GetSlice(
            _organizationId,
            CreateQuery(),
            CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Content.Count);
    }

    private InterviewsQuery CreateQuery(
        Guid? jobApplicationId = null,
        Guid? candidateId = null,
        Guid? createdByUserId = null,
        Guid? interviewerId = null,
        InterviewStatus? status = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        int page = 1,
        int pageSize = 100)
    {
        return new InterviewsQuery(
            jobApplicationId,
            candidateId,
            createdByUserId,
            interviewerId,
            status,
            from,
            to,
            page,
            pageSize);
    }

    private async Task Store(params InterviewProjection[] interviews)
    {
        await using var session = _store.LightweightSession();

        session.Store(interviews);

        await session.SaveChangesAsync();
    }

    private InterviewProjection CreateInterview(
        Guid? organizationId = null,
        Guid? candidateId = null,
        Guid? applicationId = null,
        Guid? interviewerId = null,
        Guid? createdByUserId = null,
        DateTimeOffset? scheduleAt = null,
        string timeZone = "Europe/Warsaw",
        InterviewType interviewType = InterviewType.Hr,
        InterviewStatus status = InterviewStatus.Planned)
    {
        var creatorId = createdByUserId ?? _user.Id;

        return new InterviewProjection(
            Guid.NewGuid(),
            organizationId ?? _organizationId,
            applicationId ?? Guid.NewGuid(),
            candidateId ?? Guid.NewGuid(),
            status,
            scheduleAt ?? DateTimeOffset.UtcNow,
            timeZone,
            interviewerId ?? Guid.NewGuid(),
            _user,
            InterviewFormat.Online,
            interviewType,
            creatorId,
            _user,
            "Note",
            null,
            null,
            DateTimeOffset.UtcNow,
            null);
    }
}
