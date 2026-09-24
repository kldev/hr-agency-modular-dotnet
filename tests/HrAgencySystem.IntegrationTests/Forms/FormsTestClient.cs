using System.Net.Http.Json;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Application.SystemFields.AddStandard;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;
using FormsMaps = HrAgencySystem.Api.Endpoints.Forms.Maps;
using ResponseMaps = HrAgencySystem.Api.Endpoints.FormResponses.Maps;

namespace HrAgencySystem.IntegrationTests.Forms;

/// <summary>Builds forms, catalogues and responses through the real HTTP surface.</summary>
internal sealed class FormsTestClient(HttpClient client, ITestOutputHelper output)
{
    public HttpClient Http => client;

    public async Task<IReadOnlyList<SystemField>> AddStandardFieldsAsync(Guid organizationId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsync("/api/system-fields/standard", null);
        response.EnsureSuccessStatusCode();
        Assert.NotNull(await response.ReadWithJson<StandardSystemFieldsAdded>(output));

        return await SystemFieldsAsync(organizationId);
    }

    public async Task<IReadOnlyList<SystemField>> SystemFieldsAsync(Guid organizationId)
    {
        client.WithOrganizationId(organizationId);

        var fields = await (await client.GetAsync("/api/system-fields")).ReadWithJson<List<SystemField>>(output);

        return fields!;
    }

    public async Task<Guid> CreateAsync(Guid organizationId, FormsMaps.MapCreate.CreateFormRequest? request = null)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync("/api/forms", request ?? FormsTestData.CreateRequest());
        response.EnsureSuccessStatusCode();

        return (await response.ReadWithJson<FormDefinitionCreated>(output))!.FormId;
    }

    public async Task<HttpResponseMessage> SaveDraftRawAsync(Guid organizationId, Guid formId, params FormPage[] pages)
    {
        client.WithOrganizationId(organizationId);

        return await client.PutAsJsonAsync(
            $"/api/forms/{formId}/draft",
            new FormsMaps.MapSaveDraft.SaveFormDraftRequest(pages)
        );
    }

    public async Task SaveDraftAsync(Guid organizationId, Guid formId, params FormPage[] pages) =>
        (await SaveDraftRawAsync(organizationId, formId, pages)).EnsureSuccessStatusCode();

    public async Task<FormPublished> PublishAsync(Guid organizationId, Guid formId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsync($"/api/forms/{formId}/publish", null);
        response.EnsureSuccessStatusCode();

        return (await response.ReadWithJson<FormPublished>(output))!;
    }

    /// <summary>Creates, lays out and publishes a form in one go.</summary>
    public async Task<Guid> PublishedAsync(
        Guid organizationId,
        FormsMaps.MapCreate.CreateFormRequest? request,
        params FormPage[] pages
    )
    {
        var formId = await CreateAsync(organizationId, request);
        await SaveDraftAsync(organizationId, formId, pages);
        await PublishAsync(organizationId, formId);

        return formId;
    }

    public async Task<FormDefinitionView?> GetAsync(Guid organizationId, Guid formId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync($"/api/forms/{formId}");

        return response.IsSuccessStatusCode ? await response.ReadWithJson<FormDefinitionView>(output) : null;
    }

    public async Task<FormVersion?> VersionAsync(Guid organizationId, Guid formId, int version)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync($"/api/forms/{formId}/versions/{version}");

        return response.IsSuccessStatusCode ? await response.ReadWithJson<FormVersion>(output) : null;
    }

    public async Task<HttpResponseMessage> StartRawAsync(Guid organizationId, Guid formId, Guid workerId)
    {
        client.WithOrganizationId(organizationId);

        return await client.PostAsJsonAsync(
            "/api/form-responses",
            new ResponseMaps.MapStart.StartFormResponseRequest(formId, SubjectKinds.Worker, workerId)
        );
    }

    public async Task<FormResponseStarted> StartAsync(Guid organizationId, Guid formId, Guid workerId)
    {
        var response = await StartRawAsync(organizationId, formId, workerId);

        if (!response.IsSuccessStatusCode)
            output.WriteLine(await response.Content.ReadAsStringAsync());

        response.EnsureSuccessStatusCode();

        return (await response.ReadWithJson<FormResponseStarted>(output))!;
    }

    public async Task<HttpResponseMessage> SaveAnswersRawAsync(Guid organizationId, Guid responseId, params FieldAnswer[] answers)
    {
        client.WithOrganizationId(organizationId);

        return await client.PutAsJsonAsync(
            $"/api/form-responses/{responseId}/draft",
            new ResponseMaps.AnswersRequest(answers)
        );
    }

    public async Task<HttpResponseMessage> SubmitRawAsync(Guid organizationId, Guid responseId, params FieldAnswer[] answers)
    {
        client.WithOrganizationId(organizationId);

        return await client.PostAsJsonAsync(
            $"/api/form-responses/{responseId}/submit",
            new ResponseMaps.AnswersRequest(answers)
        );
    }

    public async Task SubmitAsync(Guid organizationId, Guid responseId, params FieldAnswer[] answers) =>
        (await SubmitRawAsync(organizationId, responseId, answers)).EnsureSuccessStatusCode();

    public async Task<HttpResponseMessage> CorrectRawAsync(
        Guid organizationId,
        Guid responseId,
        string reason,
        params FieldAnswer[] answers
    )
    {
        client.WithOrganizationId(organizationId);

        return await client.PostAsJsonAsync(
            $"/api/form-responses/{responseId}/correct",
            new ResponseMaps.MapCorrect.CorrectFormResponseRequest(answers, reason)
        );
    }

    public async Task<FormResponseView?> ResponseAsync(Guid organizationId, Guid responseId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync($"/api/form-responses/{responseId}");

        return response.IsSuccessStatusCode ? await response.ReadWithJson<FormResponseView>(output) : null;
    }

    public async Task<List<FormResponseProjection>> ForWorkerAsync(Guid organizationId, Guid workerId)
    {
        client.WithOrganizationId(organizationId);

        var list = await (await client.GetAsync($"/api/subjects/worker/{workerId}/form-responses"))
            .ReadWithJson<List<FormResponseProjection>>(output);

        return list!;
    }
}
