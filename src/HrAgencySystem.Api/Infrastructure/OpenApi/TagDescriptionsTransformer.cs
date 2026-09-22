using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HrAgencySystem.Api.Infrastructure.OpenApi;

/// <summary>
/// Fills the document's <c>tags</c> section from <see cref="ApiTags.All"/>. Only tags some
/// operation actually carries are listed, so an excluded group (the internal routes) does not
/// appear as an empty heading.
/// </summary>
internal sealed class TagDescriptionsTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var used = document
            .Paths.Values.SelectMany(path =>
                path.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>()
            )
            .SelectMany(operation =>
                operation.Tags ?? Enumerable.Empty<OpenApiTagReference>()
            )
            .Select(tag => tag.Name)
            .ToHashSet();

        document.Tags = new HashSet<OpenApiTag>(
            ApiTags
                .All.Where(tag => used.Contains(tag.Name))
                .Select(tag => new OpenApiTag { Name = tag.Name, Description = tag.Description })
        );

        return Task.CompletedTask;
    }
}

internal static class OpenApiOptionsExtensions
{
    extension(OpenApiOptions options)
    {
        /// <summary>A description above every group in Scalar - see <see cref="ApiTags"/>.</summary>
        public OpenApiOptions AddTagDescriptions() =>
            options.AddDocumentTransformer<TagDescriptionsTransformer>();
    }
}
