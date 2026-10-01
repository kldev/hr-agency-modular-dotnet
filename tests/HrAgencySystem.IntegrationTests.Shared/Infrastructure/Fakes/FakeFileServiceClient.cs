using System.Collections.Concurrent;
using System.Security.Cryptography;
using HrAgencySystem.FileService.Contracts;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Fakes;

/// <summary>
/// The file service, in memory. It keeps the two guarantees the real one makes and that a test could
/// otherwise never fail: a file belongs to one organization, and another organization is told it does
/// not exist rather than that it may not look.
/// <para>
/// The alternative - standing up RustFS and a second process for the suite - would cost every test
/// the startup, and none of them read a byte back from S3. The real path is walked by hand through
/// the compose stack.
/// </para>
/// </summary>
public sealed class FakeFileServiceClient : IFileServiceClient
{
    private readonly ConcurrentDictionary<Guid, StoredEntry> _files = new();
    private int _uploads;

    /// <summary>
    /// How many uploads this double has been asked for. Registered as a singleton, so a test that
    /// wants to prove a request never reached the file service snapshots it either side of the call.
    /// </summary>
    public int UploadCount => Volatile.Read(ref _uploads);

    public Task<FileDescriptor> UploadAsync(
        Guid organizationId,
        FileOwnerRef owner,
        Guid uploadedBy,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct
    )
    {
        using var buffer = new MemoryStream();
        content.CopyTo(buffer);

        Interlocked.Increment(ref _uploads);

        var bytes = buffer.ToArray();
        var fileId = Guid.NewGuid();

        var descriptor = new FileDescriptor(
            fileId,
            fileName,
            contentType,
            bytes.Length,
            Convert.ToHexStringLower(SHA256.HashData(bytes)),
            DateTimeOffset.UtcNow
        );

        _files[fileId] = new StoredEntry(organizationId, descriptor, bytes);

        return Task.FromResult(descriptor);
    }

    public Task<FileContent?> DownloadAsync(Guid organizationId, Guid fileId, CancellationToken ct)
    {
        var entry = Find(organizationId, fileId);

        if (entry is null)
            return Task.FromResult<FileContent?>(null);

        return Task.FromResult<FileContent?>(
            new FileContent(
                new MemoryStream(entry.Bytes),
                entry.Descriptor.FileName,
                entry.Descriptor.ContentType,
                entry.Descriptor.Size
            )
        );
    }

    public Task<FileDescriptor?> GetAsync(Guid organizationId, Guid fileId, CancellationToken ct) =>
        Task.FromResult(Find(organizationId, fileId)?.Descriptor);

    public Task DeleteAsync(Guid organizationId, Guid fileId, Guid deletedBy, CancellationToken ct)
    {
        if (Find(organizationId, fileId) is not null)
            _files.TryRemove(fileId, out _);

        return Task.CompletedTask;
    }

    private StoredEntry? Find(Guid organizationId, Guid fileId) =>
        _files.TryGetValue(fileId, out var entry) && entry.OrganizationId == organizationId
            ? entry
            : null;

    private sealed record StoredEntry(Guid OrganizationId, FileDescriptor Descriptor, byte[] Bytes);
}
