using System.Net;
using System.Text;
using HrAgencySystem.Api.Infrastructure.FileServiceClient;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.UnitTests.Files;

public class HttpFileServiceClientTests
{
    private const string Reason = "The file extension does not match its content type.";

    [Fact]
    public async Task A_refused_upload_is_the_uploaders_mistake_not_an_outage()
    {
        var client = Client(HttpStatusCode.BadRequest, $$"""{"reason":"{{Reason}}"}""");

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => Upload(client));

        Assert.Equal(Reason, exception.Message);
    }

    [Fact]
    public async Task A_failing_file_service_is_still_reported_as_unavailable()
    {
        var client = Client(HttpStatusCode.InternalServerError, "{}");

        await Assert.ThrowsAsync<FileServiceException>(() => Upload(client));
    }

    private static Task<FileDescriptor> Upload(HttpFileServiceClient client) =>
        client.UploadAsync(
            Guid.NewGuid(),
            new FileOwnerRef(FileOwnerKinds.User, Guid.NewGuid()),
            Guid.NewGuid(),
            new MemoryStream([1, 2, 3]),
            "avatar.txt",
            "image/png",
            CancellationToken.None
        );

    private static HttpFileServiceClient Client(HttpStatusCode status, string body) =>
        new(
            new HttpClient(new StubHandler(status, body)) { BaseAddress = new Uri("http://files") },
            new FileServiceTokenFactory(
                Options.Create(
                    new FileServiceClientConfig
                    {
                        Secret = "unit-test-file-service-secret-at-least-32-bytes",
                    }
                )
            ),
            NullLogger<HttpFileServiceClient>.Instance
        );

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                }
            );
    }
}
