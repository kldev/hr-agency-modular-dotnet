using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Candidates;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Candidates;

[Collection(IntegrationCollection.Name)]
public class CandidateGetSliceTests(
    IntegrationEnvironment environment,
    ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _authorId = Guid.NewGuid();
    private readonly string _note = "Has been without work for 6 months.";
    
    [Fact]
    public async Task ShouldReturnEmptyCandidateSlice()
    {
        await Eventually.AssertAsync(async () =>
        {
            var result = await CandidateClient.GetSliceAsync(
                organizationId:_organizationId,
                page: 1,
                pageSize: 10);

            Assert.Empty(result.Content);
            Assert.False(result.HasMore);
        });
    }

    [Fact]
    public async Task ShouldGetCandidateSlice()
    {
        var created = await CreateCandidate();

        await Eventually.AssertAsync(async () =>
        {
            var result = await CandidateClient.GetSliceAsync(
                _organizationId,
                page: 1,
                pageSize: 10);

            var candidate = Assert.Single(
                result.Content,
                x => x.Id == created.CandidateId);

            Assert.Equal(_organizationId, candidate.OrgId);
            Assert.Equal("james@newtest.com", candidate.Email);
            Assert.Equal("James", candidate.FirstName);
            Assert.Equal("Connor", candidate.LastName);
            Assert.Equal(_note, candidate.Note);
            Assert.Equal("+49 909 123 123", candidate.PhoneNumber);
            Assert.Equal(CandidateSource.Indeed, candidate.Source);
            Assert.Equal(_authorId, candidate.CreatedBy!.Id);

            Assert.False(result.HasMore);
        });
    }

    [Fact]
    public async Task ShouldReturnAllCandidates()
    {
        var first = await CreateCandidate();

        var second = await CreateCandidate(email: "second@test.com");

        var third = await CreateCandidate(email: "third@test.com");

        await CreateCandidate(organizationId: Guid.NewGuid());
        await CreateCandidate(organizationId: Guid.NewGuid(), email: "second@test.com");
        await CreateCandidate(organizationId: Guid.NewGuid(), email: "third@test.com");

        await Eventually.AssertAsync(async () =>
        {
            var result = await CandidateClient.GetSliceAsync(
                _organizationId,
                page: 1,
                pageSize: 10);

            Assert.Equal(3, result.Content.Count);
            Assert.False(result.HasMore);

            Assert.Contains(
                result.Content,
                x => x.Id == first.CandidateId);

            Assert.Contains(
                result.Content,
                x => x.Id == second.CandidateId);

            Assert.Contains(
                result.Content,
                x => x.Id == third.CandidateId);
        });
    }

    [Fact]
    public async Task ShouldRespectPageSize()
    {
        await CreateCandidate();
        await CreateCandidate(organizationId: Guid.NewGuid());
        await CreateCandidate(organizationId: Guid.NewGuid());

        await Eventually.AssertAsync(async () =>
        {
            var result = await CandidateClient.GetSliceAsync(
                _organizationId,
                page: 1,
                pageSize: 2);

            Assert.Single(result.Content);
            Assert.False(result.HasMore);
        });
    }

    [Fact]
    public async Task ShouldReturnNextSlice()
    {
        var first = await CreateCandidate();

        var second = await CreateCandidate(
            email: "second@test.com");

        var third = await CreateCandidate(
            email: "third@test.com");

        await Eventually.AssertAsync(async () =>
        {
            var firstPage = await CandidateClient.GetSliceAsync(
                _organizationId,
                page: 1,
                pageSize: 2);

            Assert.Equal(2, firstPage.Content.Count);
            Assert.True(firstPage.HasMore);

            var secondPage = await CandidateClient.GetSliceAsync(
                _organizationId,
                page: 2,
                pageSize: 2);

            Assert.Single(secondPage.Content);
            Assert.False(secondPage.HasMore);

            var secondPageCandidate = Assert.Single(secondPage.Content);

            Assert.Equal(
                first.CandidateId,
                secondPageCandidate.Id);

            Assert.DoesNotContain(
                secondPage.Content,
                candidate => firstPage.Content.Any(x => x.Id == candidate.Id));
        });
    }

    [Fact]
    public async Task ShouldReturnHasMoreFalseWhenPageContainsLastCandidates()
    {
        await CreateCandidate();
        await CreateCandidate(email: "second@test.com");

        await Eventually.AssertAsync(async () =>
        {
            var result = await CandidateClient.GetSliceAsync(
                _organizationId,
                page: 1,
                pageSize: 2);

            Assert.Equal(2, result.Content.Count);
            Assert.False(result.HasMore);
        });
    }

    private async Task<CandidateCreated> CreateCandidate(
        Guid? organizationId = null,
        Guid? createdByUserId = null,
        string? email = null )
    {
        return await CandidateClient.Create(
            organizationId ?? _organizationId,
            Email: email ?? "james@newtest.com",
            FirstName: "James",
            LastName: "Connor",
            Note: _note,
            Phone: "+49 909 123 123",
            source: CandidateSource.Indeed,
            createdByUserId: createdByUserId ?? _authorId);
    }
}