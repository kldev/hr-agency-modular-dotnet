using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public static class HttpExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<T?> ReadWithJson<T>(
        this HttpResponseMessage response,
        ITestOutputHelper? output = null)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (output == null)
            return string.IsNullOrWhiteSpace(content) ? default : JsonSerializer.Deserialize<T>(content, JsonOptions);
        
        output.WriteLine($"Status: {response.StatusCode}");
        output.WriteLine($"Content: {content}");

        return string.IsNullOrWhiteSpace(content) ? default : JsonSerializer.Deserialize<T>(content, JsonOptions);
    }
}