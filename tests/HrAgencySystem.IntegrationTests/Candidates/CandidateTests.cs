using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Candidate.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.SharedKernel.Port;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Candidates;

[Collection(IntegrationCollection.Name)]
public class CandidateTests(
    IntegrationEnvironment environment,
    ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _authorId = Guid.NewGuid();
    private readonly string _note = "Has been without work for 6 months.";
    private readonly string _updatedNote = "Has been without work for 6 months. Lets help him. Nothing new";

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanCandidates();
    }

    [Fact]
    public async Task ShouldCreateCandidate()
    {
        var result = await CreateCandidate();

        Assert.Equal(_organizationId, result.OrganizationId);
        Assert.Equal("James", result.FirstName);
        Assert.Equal("Connor", result.LastName);
        Assert.Equal(_note, result.Note);
        Assert.Equal("+49 909 123 123", result.Phone);
        Assert.Equal(CandidateSource.Indeed, result.Source);
        Assert.Equal(_authorId, result.CreatedBy!.Id);
    }

    [Fact]
    public async Task ShouldUpdateCandidate()
    {
        var created = await CreateCandidate();

        var modifiedBy = Guid.NewGuid();
        var updateResult = await CandidateClient.Update(
            candidateId: created.CandidateId,
            organizationId: _organizationId,
            FirstName: "Paul",
            LastName: "Henderson",
            Note: _updatedNote,
            Phone: "+49 909 123 321",
            modifiedByUserId: modifiedBy);

        Assert.Equal(_organizationId, updateResult.OrganizationId);
        Assert.Equal("Paul", updateResult.FirstName);
        Assert.Equal("Henderson", updateResult.LastName);
        Assert.Equal(_updatedNote, updateResult.Note);
        Assert.Equal("+49 909 123 321", updateResult.Phone);


        Assert.Equal(modifiedBy, updateResult.ModifiedBy!.Id);

        await Eventually.AssertAsync(async () =>
        {
            var candidate = await CandidateClient.GetAsync(
                _organizationId,
                created.CandidateId);

            Assert.NotNull(candidate);
            Assert.Equal("Paul", candidate.FirstName);
            Assert.Equal("james@newtest.com", candidate.Email);
            Assert.Equal("Henderson", candidate.LastName);
            Assert.Equal(
                _updatedNote,
                candidate.Note);
            Assert.Equal("+49 909 123 321", candidate.PhoneNumber);
            Assert.Equal(modifiedBy, candidate.ModifiedBy!.Id);
        });
    }

    [Fact]
    public async Task ShouldCreateCandidateAndGetProjection()
    {
        var result = await CreateCandidate(organizationId: _organizationId);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await CandidateClient.GetAsync(_organizationId, result.CandidateId);

            Assert.NotNull(projection);
            Assert.Equal(_organizationId, projection.OrgId);
            Assert.Equal("James", projection.FirstName);
            Assert.Equal("Connor", projection.LastName);
            Assert.Equal(_note, projection.Note);
            Assert.Equal("+49 909 123 123", projection.PhoneNumber);
            Assert.Equal(CandidateSource.Indeed, projection.Source);
            Assert.Equal(_authorId, projection.CreatedBy!.Id);
            Assert.Equal("james@newtest.com", projection.Email);
        });
    }

    [Fact]
    public async Task ShouldNotGetOtherOrganizationCandidate()
    {
        var result = await CreateCandidate();

        var otherOrganization = Guid.NewGuid();
        
        var projection = await CandidateClient.GetAsync(otherOrganization, result.CandidateId);
        Assert.Null(projection);
    }

    [Fact]
    public async Task ShouldNotCreateCandidateWithDuplicateEmail()
    {
        await CreateCandidate(organizationId :_organizationId);

        var request = new CreateCandidateRequest(
            "james@newtest.com", "", "", "", CandidateSource.JustJoinIt, "");

        Client.WithOrganizationId(_organizationId);
        
        var response =
            await Client.PostAsJsonAsync(CandidateTestClient.BaseUrl, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(ICandidateEmailReservationRepository.EmailAlreadyExistsMessage, problem.Detail);
    }


    [Fact] 
    public async Task ShouldAllowSameCandidateEmailForDifferentOrganizations()
    {
        var result = await CreateCandidate(organizationId :_organizationId);

        Assert.Equal(_organizationId, result.OrganizationId);
        
        var otherOrganization  = Guid.NewGuid();
        var resultOther = await CreateCandidate(organizationId :otherOrganization);
        Assert.Equal(otherOrganization, resultOther.OrganizationId);
        
        Assert.Equal(result.Email, resultOther.Email);
    }
    
    [Fact]
    public async Task ShouldNotUpdateCandidateFromOtherOrganization()
    {
        var created = await CandidateClient.Create(
            organizationId: _organizationId,
            Email: "james@newtest.com",
            FirstName: "James",
            LastName: "Connor",
            Note: _note,
            Phone: "+49 909 123 123",
            source: CandidateSource.Indeed,
            createdByUserId: _authorId);

        var otherOrganizationId = Guid.NewGuid();
        var modifiedBy = Guid.NewGuid();

        var updateRequest = new MapUpdate.UpdateCandidateRequest(
            "+49 999 999 999",
            "Hacked",
            "Candidate",
            "This should not be saved");


        Client.WithOrganizationId(otherOrganizationId);
        Client.WithUserId(modifiedBy);
        var response =
            await Client.PutAsJsonAsync(CandidateTestClient.BaseUrl + "/" + created.CandidateId, updateRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.ReadWithJson<ProblemDetails>();
        Assert.NotNull(problemDetails);

        Assert.Equal(IOrganizationChecker.OrganizationCheckMessage, problemDetails.Detail);


        await Eventually.AssertAsync(async () =>
        {
            var candidate = await CandidateClient.GetAsync(
                _organizationId,
                created.CandidateId);

            Assert.NotNull(candidate);

            Assert.Equal(_organizationId, candidate.OrgId);
            Assert.Equal("James", candidate.FirstName);
            Assert.Equal("Connor", candidate.LastName);
            Assert.Equal(
                _note,
                candidate.Note);
            Assert.Equal("+49 909 123 123", candidate.PhoneNumber);
            Assert.Equal(CandidateSource.Indeed, candidate.Source);
            Assert.Equal(_authorId, candidate.CreatedBy!.Id);
        });
    }

    private async Task<CandidateCreated> CreateCandidate(
        Guid? organizationId = null,
        Guid? createdByUserId = null)
    {
        return await CandidateClient.Create(
            organizationId ?? _organizationId,
            Email: "james@newtest.com",
            FirstName: "James",
            LastName: "Connor",
            Note: _note,
            Phone: "+49 909 123 123",
            source: CandidateSource.Indeed,
            createdByUserId: createdByUserId ?? _authorId);
    }
}