using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Documents;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapTags
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group.MapGet("/api/suggestion/tags", Handler).WithSummary("Get tags (limit 25)");
    }
    
    private static async Task<IResult> Handler(ITagSuggestionRepository repository, string? search, TagCategory? category,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(search) && !category.HasValue)
        {
            return TypedResults.BadRequest(new ProblemDetails()
            {
                Title = "No search or category parameter were provided.",
                Status = StatusCodes.Status400BadRequest, Detail = $""
            });
        }
        
        var result = await repository.GetSuggestions(search ?? "", category, ct);
        return TypedResults.Ok(result);
    }
}