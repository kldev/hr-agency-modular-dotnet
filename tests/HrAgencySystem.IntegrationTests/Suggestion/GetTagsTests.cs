using System.Net;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Documents;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Suggestion;

[Collection(IntegrationCollection.Name)]
public sealed class GetTagsTests(IntegrationEnvironment environment, ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private async Task<IReadOnlyList<Tag>> GetTagsSuggestions(string search = "", TagCategory? category = null)
    {
        var url = "/api/suggestion/tags?search=" + search;
        if (category.HasValue)
        {
            url += "&category=" + category;
        }

        var response = await Client.GetAsync(url);
    
        response.EnsureSuccessStatusCode();
        
        var result = (await response.ReadWithJson<IReadOnlyList<Tag>>(OutputHelper))!;
   
        return result;
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenNoSearchOrCategoryProvided()
    {
        var response = await Client.GetAsync("/api/suggestion/tags");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadWithJson<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("No search or category parameter were provided.", problem.Title);
    }
    
    [Fact]
    public async Task ShouldGetTagsFilterBySearchQuery()
    {
        var response = await GetTagsSuggestions("Azure");

        Assert.NotEmpty(response);
        Assert.Equal(12, response.Count);
    }
    
    [Fact]
    public async Task ShouldGetTagsFilterByCategoryQuery()
    {
        var response = await GetTagsSuggestions("", TagCategory.DrivingLicense);

        Assert.NotEmpty(response);
        Assert.Equal(11, response.Count);
    }
    
    [Fact]
    public async Task ShouldGetTagsFilterByCategoryAndSearchQuery()
    {
        var response = await GetTagsSuggestions("Type", TagCategory.SkillIt);

        Assert.NotEmpty(response);
        Assert.Single(response);
    }
}