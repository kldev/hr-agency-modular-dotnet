using System.Net;
using System.Net.Http.Headers;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Api.Infrastructure.FileServiceClient;

public sealed class HttpFileServiceClient(
    HttpClient http,
    FileServiceTokenFactory tokens,
    ILogger<HttpFileServiceClient> logger
) : IFileServiceClient
{
    public async Task<FileDescriptor> UploadAsync(
        Guid organizationId,
        FileOwnerRef owner,
        Guid uploadedBy,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct
    )
    {
        using var form = new MultipartFormDataContent();
        using var file = new StreamContent(content);
        file.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

        form.Add(file, "file", fileName);
        form.Add(new StringContent(owner.Kind), "ownerKind");
        form.Add(new StringContent(owner.Id.ToString()), "ownerId");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/files");
        request.Content = form;
        request.Headers.Authorization = Bearer(organizationId, uploadedBy);

        using var response = await Send(request, ct);

        // A refused file is the uploader's mistake, not an outage: a 400 with the reason, never the
        // 503 a FileServiceException becomes - that would page somebody over a renamed .exe.
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var rejection = await response.Content.ReadFromJsonAsync<UploadRejected>(ct);
            throw new BusinessRuleException(
                rejection?.Reason ?? FileServiceException.RejectedMessage
            );
        }

        EnsureSuccess(response);

        return await response.Content.ReadFromJsonAsync<FileDescriptor>(ct)
            ?? throw new FileServiceException(FileServiceException.UnavailableMessage);
    }

    public async Task<FileContent?> DownloadAsync(
        Guid organizationId,
        Guid fileId,
        CancellationToken ct
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/files/{fileId}/content");
        request.Headers.Authorization = Bearer(organizationId, Guid.Empty);

        // Not disposed here on purpose: the content stream outlives the response and is handed to
        // the caller, which streams it to the browser and disposes it then.
        var response = await Send(request, ct, HttpCompletionOption.ResponseHeadersRead);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            response.Dispose();
            return null;
        }

        EnsureSuccess(response);

        var stream = await response.Content.ReadAsStreamAsync(ct);

        return new FileContent(
            stream,
            response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? fileId.ToString(),
            response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream",
            response.Content.Headers.ContentLength ?? 0
        );
    }

    public async Task<FileDescriptor?> GetAsync(
        Guid organizationId,
        Guid fileId,
        CancellationToken ct
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/files/{fileId}");
        request.Headers.Authorization = Bearer(organizationId, Guid.Empty);

        using var response = await Send(request, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        EnsureSuccess(response);

        return await response.Content.ReadFromJsonAsync<FileDescriptor>(ct);
    }

    public async Task DeleteAsync(
        Guid organizationId,
        Guid fileId,
        Guid deletedBy,
        CancellationToken ct
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/files/{fileId}");
        request.Headers.Authorization = Bearer(organizationId, deletedBy);

        using var response = await Send(request, ct);

        // Already gone is the outcome the caller wanted.
        if (response.StatusCode == HttpStatusCode.NotFound)
            return;

        EnsureSuccess(response);
    }

    private AuthenticationHeaderValue Bearer(Guid organizationId, Guid actorId) =>
        new("Bearer", tokens.Create(organizationId, actorId));

    private async Task<HttpResponseMessage> Send(
        HttpRequestMessage request,
        CancellationToken ct,
        HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead
    )
    {
        try
        {
            return await http.SendAsync(request, completion, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // The service being down is not the same as the document being missing, and saying so
            // is the difference between "storage is unavailable" and a pointless hunt for a file
            // that is exactly where it should be.
            logger.LogError(
                ex,
                "The file service could not be reached at {Uri}.",
                request.RequestUri
            );
            throw new FileServiceException(FileServiceException.UnavailableMessage, ex);
        }
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        throw new FileServiceException(
            $"{FileServiceException.UnavailableMessage} ({(int)response.StatusCode})"
        );
    }

    private sealed record UploadRejected(string Reason);
}
