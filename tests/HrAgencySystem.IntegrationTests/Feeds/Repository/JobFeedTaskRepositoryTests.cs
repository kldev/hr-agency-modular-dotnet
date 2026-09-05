using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Feeds.Model;
using HrAgencySystem.Recruitment.Feeds.Persistence;
using HrAgencySystem.Recruitment.Feeds.Port;
using Npgsql;

namespace HrAgencySystem.IntegrationTests.Feeds.Repository;


[Collection(PostgresCollection.Name)]
public sealed class JobFeedTaskRepositoryTests(PostgresFixture postgres) : IAsyncLifetime
{
    private NpgsqlDataSource _dataSource = null!;
    private IJobFeedTaskRepository _repository = null!;
    private IJobFeedTaskQueue _queue = null!;
    
    private async Task CleanDatabase()
    {
        await using var connection =
            await postgres.DataSource.OpenConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              TRUNCATE TABLE jobs.job_feed_tasks;
                              """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task InitializeAsync()
    {
        _dataSource = postgres.DataSource;
        
        await CreateSchema();
        await CleanDatabase();

        

        _repository = new JobFeedTaskRepository(_dataSource);
        _queue = new JobFeedTaskQueue(_dataSource);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Save_ShouldPersistTask()
    {
        // Arrange
        var task = CreateTask();

        // Act
        await _repository.Save(task, CancellationToken.None);

        // Assert
        var stored = await GetTask(task.Id);

        Assert.NotNull(stored);
        Assert.Equal(task.Id, stored.Id);
        Assert.Equal(task.OrganizationId, stored.OrganizationId);
        Assert.Equal(JobFeedTaskStatus.Pending, stored.Status);
        Assert.Equal(0, stored.Attempts);
        Assert.Null(stored.StartedAt);
        Assert.Null(stored.CompletedAt);
        Assert.Null(stored.ErrorMessage);
    }

    [Fact]
    public async Task Save_ShouldDoNothing_WhenTaskWithSameIdAlreadyExists()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        // Act
        await _repository.Save(task, CancellationToken.None);

        // Assert
        var count = await CountTasks(task.Id);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task BatchSave_ShouldPersistAllTasks()
    {
        // Arrange
        var tasks = new[]
        {
            CreateTask(),
            CreateTask(),
            CreateTask()
        };

        // Act
        await _repository.BatchSave(tasks, CancellationToken.None);

        // Assert
        foreach (var task in tasks)
        {
            var stored = await GetTask(task.Id);

            Assert.NotNull(stored);
            Assert.Equal(task.Id, stored.Id);
            Assert.Equal(task.OrganizationId, stored.OrganizationId);
            Assert.Equal(JobFeedTaskStatus.Pending, stored.Status);
            Assert.Equal(0, stored.Attempts);
        }
    }

    [Fact]
    public async Task BatchSave_ShouldDoNothingForDuplicateTasks()
    {
        // Arrange
        var existingTask = CreateTask();

        await _repository.Save(existingTask, CancellationToken.None);

        var newTask = CreateTask();

        var tasks = new[]
        {
            existingTask,
            newTask
        };

        // Act
        await _repository.BatchSave(tasks, CancellationToken.None);

        // Assert
        Assert.Equal(1, await CountTasks(existingTask.Id));
        Assert.Equal(1, await CountTasks(newTask.Id));

        Assert.Equal(2, await CountAllTasks());
    }

    [Fact]
    public async Task Fetch_ShouldReturnPendingTasks()
    {
        // Arrange
        var pending1 = CreateTask();
        var pending2 = CreateTask();

        await _repository.BatchSave(
            [pending1, pending2],
            CancellationToken.None);

        // Act
        var result = await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Contains(
            result,
            task => task.Id == pending1.Id);

        Assert.Contains(
            result,
            task => task.Id == pending2.Id);
    }

    [Fact]
    public async Task Fetch_ShouldRespectBatchSize()
    {
        // Arrange
        var tasks = new[]
        {
            CreateTask(),
            CreateTask(),
            CreateTask()
        };

        await _repository.BatchSave(
            tasks,
            CancellationToken.None);

        // Act
        var result = await _queue.Fetch(
            batchSize: 2,
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Fetch_ShouldReturnTasksInCreatedAtOrder()
    {
        // Arrange
        var first = CreateTask();
        var second = CreateTask();
        var third = CreateTask();

        await _repository.BatchSave(
            [first, second, third],
            CancellationToken.None);

        await SetCreatedAt(first.Id, DateTimeOffset.UtcNow.AddMinutes(-3));
        await SetCreatedAt(second.Id, DateTimeOffset.UtcNow.AddMinutes(-2));
        await SetCreatedAt(third.Id, DateTimeOffset.UtcNow.AddMinutes(-1));

        // Act
        var result = await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            [first.Id, second.Id, third.Id],
            result.Select(x => x.Id).ToArray());
    }

    [Fact]
    public async Task Fetch_ShouldReturnOnlyPendingTasks()
    {
        // Arrange
        var pending = CreateTask();
        var processing = CreateTask();

        await _repository.BatchSave(
            [pending, processing],
            CancellationToken.None);

        await SetStatus(
            processing.Id,
            JobFeedTaskStatus.Processing);

        // Act
        var result = await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Assert
        var task = Assert.Single(result);

        Assert.Equal(pending.Id, task.Id);
    }

    [Fact]
    public async Task Fetch_ShouldChangeStatusToProcessing()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        // Act
        var result = await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Assert
        var fetched = Assert.Single(result);

        Assert.Equal(
            JobFeedTaskStatus.Processing,
            fetched.Status);

        var stored = await GetTask(task.Id);

        Assert.NotNull(stored);
        Assert.Equal(
            JobFeedTaskStatus.Processing,
            stored.Status);
    }

    [Fact]
    public async Task Fetch_ShouldIncrementAttempts()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        // Act
        var result = await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Assert
        var fetched = Assert.Single(result);

        Assert.Equal(1, fetched.Attempts);

        var stored = await GetTask(task.Id);

        Assert.NotNull(stored);
        Assert.Equal(1, stored.Attempts);
    }

    [Fact]
    public async Task Fetch_ShouldSetStartedAt()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        // Act
        var result = await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Assert
        var fetched = Assert.Single(result);

        Assert.NotNull(fetched.StartedAt);
    }

    [Fact]
    public async Task Fetch_ShouldNotReturnCompletedTasks()
    {
        // Arrange
        var pending = CreateTask();
        var completed = CreateTask();

        await _repository.BatchSave(
            [pending, completed],
            CancellationToken.None);

        await SetCompleted(completed.Id);

        // Act
        var result = await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Assert
        var task = Assert.Single(result);

        Assert.Equal(pending.Id, task.Id);
    }

    [Fact]
    public async Task Fetch_ShouldNotReturnAlreadyProcessingTasks()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        // First worker claims task.
        var firstResult = await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        Assert.Single(firstResult);

        // Act
        // Second worker tries to fetch pending tasks.
        var secondResult = await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Assert
        Assert.Empty(secondResult);
    }

    [Fact]
    public async Task Fetch_ShouldReturnEmpty_WhenThereAreNoPendingTasks()
    {
        // Act
        var result = await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Fetch_ShouldReturnEmpty_WhenBatchSizeIsZero()
    {
        // Arrange
        await _repository.Save(
            CreateTask(),
            CancellationToken.None);

        // Act
        var result = await _queue.Fetch(
            batchSize: 0,
            CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task MarkFailed_ShouldSetStatusToPending_WhenAttemptsAreBelowLimit()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Act
        await _repository.MarkFailed(
            task.Id,
            "Feed processing failed",
            CancellationToken.None);

        // Assert
        var stored = await GetTask(task.Id);

        Assert.NotNull(stored);
        Assert.Equal(JobFeedTaskStatus.Pending, stored.Status);
        Assert.Equal(2, stored.Attempts);
        Assert.Equal("Feed processing failed", stored.ErrorMessage);
    }
    
    [Fact]
    public async Task MarkFailed_ShouldSetStatusToFailed_WhenAttemptsReachLimit()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        // First fetch: attempts = 1
        await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // First failure: attempts = 2, status = PENDING
        await _repository.MarkFailed(
            task.Id,
            "First failure",
            CancellationToken.None);

        // Fetch again: attempts = 3, status = PROCESSING
        await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Act
        // Failure increments attempts to 4 and should result in FAILED.
        await _repository.MarkFailed(
            task.Id,
            "Final failure",
            CancellationToken.None);

        // Assert
        var stored = await GetTask(task.Id);

        Assert.NotNull(stored);
        Assert.Equal(JobFeedTaskStatus.Failed, stored.Status);
        Assert.Equal(4, stored.Attempts);
        Assert.Equal("Final failure", stored.ErrorMessage);
    }
    
    [Fact]
    public async Task MarkFailed_ShouldKeepStatusPending_WhenTaskIsNotProcessing()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        // Act
        await _repository.MarkFailed(
            task.Id,
            "Unexpected failure",
            CancellationToken.None);

        // Assert
        var stored = await GetTask(task.Id);

        Assert.NotNull(stored);
        Assert.Equal(JobFeedTaskStatus.Pending, stored.Status);
        Assert.Equal(0, stored.Attempts);
        Assert.NotEqual("Unexpected failure", stored.ErrorMessage);
    }
    
    [Fact]
    public async Task MarkCompleted_ShouldSetStatusToCompleted()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Act
        await _repository.MarkCompleted(
            task.Id,
            CancellationToken.None);

        // Assert
        var stored = await GetTask(task.Id);

        Assert.NotNull(stored);
        Assert.Equal(JobFeedTaskStatus.Completed, stored.Status);
    }
    
    [Fact]
    public async Task MarkCompleted_ShouldNotChangeAttemptsOrErrorMessage()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        await _repository.MarkFailed(
            task.Id,
            "Temporary failure",
            CancellationToken.None);

        var beforeComplete = await GetTask(task.Id);

        Assert.NotNull(beforeComplete);
        Assert.Equal(JobFeedTaskStatus.Pending, beforeComplete.Status);
        Assert.Equal(2, beforeComplete.Attempts);
        Assert.Equal("Temporary failure", beforeComplete.ErrorMessage);

        // Fetch again so task becomes PROCESSING.
        await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        // Act
        await _repository.MarkCompleted(
            task.Id,
            CancellationToken.None);

        // Assert
        var stored = await GetTask(task.Id);

        Assert.NotNull(stored);
        Assert.Equal(JobFeedTaskStatus.Completed, stored.Status);
        Assert.Equal(3, stored.Attempts);
        Assert.Equal("Temporary failure", stored.ErrorMessage);
    }
    
    [Fact]
    public async Task MarkFailed_ShouldNotChangeCompletedStatus()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        await _repository.MarkCompleted(
            task.Id,
            CancellationToken.None);

        // Act
        await _repository.MarkFailed(
            task.Id,
            "Late failure",
            CancellationToken.None);

        // Assert
        var stored = await GetTask(task.Id);

        Assert.NotNull(stored);
        Assert.Equal(JobFeedTaskStatus.Completed, stored.Status);
        Assert.Equal(1, stored.Attempts);
        Assert.NotEqual("Late failure", stored.ErrorMessage);
    }
    
    [Fact]
    public async Task MarkFailed_ShouldNotRevertFailedTaskToPending()
    {
        // Arrange
        var task = CreateTask();

        await _repository.Save(task, CancellationToken.None);

        await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        await _repository.MarkFailed(
            task.Id,
            "First failure",
            CancellationToken.None);

        await _queue.Fetch(
            batchSize: 10,
            CancellationToken.None);

        await _repository.MarkFailed(
            task.Id,
            "Final failure",
            CancellationToken.None);

        var failed = await GetTask(task.Id);

        Assert.NotNull(failed);
        Assert.Equal(JobFeedTaskStatus.Failed, failed.Status);

        // Act
        await _repository.MarkFailed(
            task.Id,
            "Another failure",
            CancellationToken.None);

        // Assert
        var stored = await GetTask(task.Id);

        Assert.NotNull(stored);
        Assert.Equal(JobFeedTaskStatus.Failed, stored.Status);
        Assert.Equal(4, stored.Attempts);
        Assert.NotEqual("Another failure", stored.ErrorMessage);
    }
    

    private JobFeedTask CreateTask()
    {
        return new JobFeedTask(
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobFeedTaskStatus.Pending,
            0,
            DateTimeOffset.UtcNow,
            null,
            null,
            null);
    }

    private async Task<JobFeedTask?> GetTask(Guid id)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command = connection.CreateCommand();

      
        
        
        command.CommandText = """
            SELECT
                id,
                organization_id,
                status,
                attempts,
                created_at,
                started_at,
                completed_at,
                error_message
            FROM jobs.job_feed_tasks
            WHERE id = @id
            """;

        command.Parameters.AddWithValue("id", id);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        var status = Enum.Parse<JobFeedTaskStatus>(
            reader.GetString(2),
            ignoreCase: true);
        
        return new JobFeedTask(
            reader.GetGuid(0),
            reader.GetGuid(1),
            status,
            reader.GetInt32(3),
            reader.GetFieldValue<DateTimeOffset>(4),
            reader.GetFieldValue<DateTimeOffset?>(5),
            reader.GetFieldValue<DateTimeOffset?>(6),
            reader.IsDBNull(7)
                ? null
                : reader.GetString(7));
    }

    private async Task<int> CountTasks(Guid id)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT count(*)
            FROM jobs.job_feed_tasks
            WHERE id = @id
            """;

        command.Parameters.AddWithValue("id", id);

        return Convert.ToInt32(
            await command.ExecuteScalarAsync());
    }

    private async Task<int> CountAllTasks()
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT count(*)
            FROM jobs.job_feed_tasks
            """;

        return Convert.ToInt32(
            await command.ExecuteScalarAsync());
    }

    private async Task SetStatus(
        Guid id,
        JobFeedTaskStatus status)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE jobs.job_feed_tasks
            SET status = @status
            WHERE id = @id
            """;

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue(
            "status",
            status.ToString().ToUpperInvariant());

        await command.ExecuteNonQueryAsync();
    }

    private async Task SetCompleted(Guid id)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE jobs.job_feed_tasks
            SET
                status = 'COMPLETED',
                completed_at = now()
            WHERE id = @id
            """;

        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }

    private async Task SetCreatedAt(
        Guid id,
        DateTimeOffset createdAt)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE jobs.job_feed_tasks
            SET created_at = @createdAt
            WHERE id = @id
            """;

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("createdAt", createdAt);

        await command.ExecuteNonQueryAsync();
    }

    private async Task CreateSchema()
    {
        await new FeedMigration(_dataSource).SeedAsync(CancellationToken.None);
    }
}
