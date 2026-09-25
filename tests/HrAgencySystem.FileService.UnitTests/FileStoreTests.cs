using System.Diagnostics.Metrics;
using HrAgencySystem.Files.Model;
using HrAgencySystem.Files.Service;
using HrAgencySystem.FileService.Application;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.FileService.Domain;
using HrAgencySystem.FileService.Infrastructure.Telemetry;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using NSubstitute;

namespace HrAgencySystem.FileService.UnitTests;

public sealed class FileStoreTests : IDisposable
{
    private static readonly Guid OrganizationId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );
    private static readonly Guid OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ActorId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly IObjectStorage _storage = Substitute.For<IObjectStorage>();
    private readonly IUploadInspector _inspector = Substitute.For<IUploadInspector>();
    private readonly ServiceProvider _services = new ServiceCollection()
        .AddMetrics()
        .BuildServiceProvider();
    private readonly IMeterFactory _meters;
    private readonly MetricCollector<long> _uploads;

    public FileStoreTests()
    {
        _meters = _services.GetRequiredService<IMeterFactory>();
        _uploads = new MetricCollector<long>(_meters, FileMetrics.MeterName, "hr.files.uploads");
    }

    public void Dispose()
    {
        _uploads.Dispose();
        _services.Dispose();
    }

    [Fact]
    public async Task ARefusedUploadNeverReachesTheBucketOrTheDatabase()
    {
        _inspector
            .Inspect(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>())
            .Returns(UploadInspector.UnsupportedTypeMessage);

        var result = await Store("payload.bin", "application/x-msdownload");

        Assert.Null(result.File);
        Assert.Equal(UploadInspector.UnsupportedTypeMessage, result.Rejection);

        await _storage.DidNotReceiveWithAnyArgs().StoreAsync(default!, default!, default!, default);
        _session.DidNotReceiveWithAnyArgs().Insert(Arg.Any<StoredFile>());
        await _session.DidNotReceiveWithAnyArgs().SaveChangesAsync();

        var measurement = Assert.Single(_uploads.GetMeasurementSnapshot());
        Assert.Equal(FileMetrics.Rejected, measurement.Tags["outcome"]);
        Assert.Equal("unsupported_type", measurement.Tags["reason"]);
    }

    [Fact]
    public async Task AnAcceptedUploadIsStoredUnderAGeneratedKeyAndHashed()
    {
        _inspector
            .Inspect(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>())
            .Returns((string?)null);
        _inspector.ExtensionFor("application/pdf").Returns(".pdf");

        var result = await Store("../../etc/umowa.pdf", "application/pdf");

        Assert.Null(result.Rejection);
        Assert.NotNull(result.File);

        // The name is cleaned before it is ever shown back.
        Assert.Equal("umowa.pdf", result.File!.FileName);

        // sha256 of "contents", checked against a value produced outside this code.
        Assert.Equal(
            "d1b2a59fbea7e20077af9f91b27e95e865061b270be03ff539ab3b73587882e8",
            result.File.Sha256
        );

        await _storage
            .Received(1)
            .StoreAsync(
                Arg.Any<FileInput>(),
                Arg.Is<string>(key =>
                    key.StartsWith(
                        $"{OrganizationId:N}/project/{OwnerId:N}/",
                        StringComparison.Ordinal
                    ) && key.EndsWith(".pdf", StringComparison.Ordinal)
                ),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        var measurement = Assert.Single(_uploads.GetMeasurementSnapshot());
        Assert.Equal(FileMetrics.Stored, measurement.Tags["outcome"]);
    }

    private async Task<StoreResult> Store(string fileName, string contentType)
    {
        var store = new FileStore(
            _session,
            _storage,
            _inspector,
            TimeProvider.System,
            new FileMetrics(_meters),
            NullLogger<FileStore>.Instance
        );

        var bytes = "contents"u8.ToArray();
        await using var content = new MemoryStream(bytes);

        return await store.StoreAsync(
            OrganizationId,
            new FileOwnerRef(FileOwnerKinds.Project, OwnerId),
            ActorId,
            content,
            bytes.Length,
            fileName,
            contentType,
            CancellationToken.None
        );
    }
}
