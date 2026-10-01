using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.LegalEntity.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.LegalEntities.Events;
using HrAgencySystem.LegalEntities.Projections;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.LegalEntities;

[Collection(IntegrationCollection.Name)]
public class LegalEntityTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    private const string BaseUrl = "/api/legal-entities";

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanLegalEntities();
    }

    [Fact]
    public async Task A_legal_entity_is_recorded_with_its_registered_details()
    {
        var organizationId = Guid.NewGuid();

        var created = await Create(
            organizationId,
            LegalEntityTestData.Request(name: "HR Agency North", taxId: "5213870274")
        );

        Assert.Equal("HR Agency North", created.Name);
        Assert.Equal("HR Agency sp. z o.o.", created.LegalName);
        Assert.Equal("5213870274", created.TaxId);
        Assert.Equal("Warszawa", created.RegisteredAddress.City);
        Assert.Equal("Anna", created.President.FirstName);

        await Eventually.AssertAsync(async () =>
        {
            var entity = await Get(organizationId, created.LegalEntityId);

            Assert.Equal("HR Agency North", entity.Name);
            Assert.Null(entity.ActiveTo);
        });
    }

    [Fact]
    public async Task Accounts_are_kept_per_purpose_and_currency()
    {
        var organizationId = Guid.NewGuid();

        var created = await Create(
            organizationId,
            LegalEntityTestData.Request(
                accounts:
                [
                    LegalEntityTestData.Account(BankAccountPurpose.Incoming, CurrencyCode.PLN),
                    LegalEntityTestData.Account(
                        BankAccountPurpose.Incoming,
                        CurrencyCode.EUR,
                        "DE89370400440532013000"
                    ),
                    LegalEntityTestData.Account(
                        BankAccountPurpose.Outgoing,
                        CurrencyCode.PLN,
                        "PL27114020040000300201355387"
                    ),
                ]
            )
        );

        Assert.Equal(3, created.BankAccounts.Count);

        var incomingEur = created.BankAccounts.Single(a =>
            a.Purpose == BankAccountPurpose.Incoming && a.Currency == CurrencyCode.EUR
        );

        Assert.Equal("DE89370400440532013000", incomingEur.Iban);
    }

    [Fact]
    public async Task The_same_account_purpose_and_currency_twice_is_refused()
    {
        var organizationId = Guid.NewGuid();

        Client.WithOrganizationId(organizationId);

        var response = await Client.PostAsJsonAsync(
            BaseUrl,
            LegalEntityTestData.Request(
                accounts:
                [
                    LegalEntityTestData.Account(BankAccountPurpose.Incoming, CurrencyCode.EUR),
                    LegalEntityTestData.Account(
                        BankAccountPurpose.Incoming,
                        CurrencyCode.EUR,
                        "DE89370400440532013000"
                    ),
                ]
            )
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// The same company entered twice is the mistake this prevents, and it is scoped to the
    /// organization: two agencies may each trade under a number the other never sees.
    /// </summary>
    [Fact]
    public async Task A_tax_id_is_used_once_inside_an_organization()
    {
        var organizationId = Guid.NewGuid();
        var taxId = LegalEntityTestData.NewTaxId();

        await Create(organizationId, LegalEntityTestData.Request(taxId: taxId));

        Client.WithOrganizationId(organizationId);

        var duplicate = await Client.PostAsJsonAsync(
            BaseUrl,
            LegalEntityTestData.Request(taxId: taxId)
        );

        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);

        // Another agency using the same number is not our business.
        var elsewhere = await Create(Guid.NewGuid(), LegalEntityTestData.Request(taxId: taxId));

        Assert.Equal(taxId, elsewhere.TaxId);
    }

    [Fact]
    public async Task Editing_replaces_the_details_and_the_accounts()
    {
        var organizationId = Guid.NewGuid();
        var created = await Create(organizationId, LegalEntityTestData.Request());

        Client.WithOrganizationId(organizationId);

        var response = await Client.PutAsJsonAsync(
            $"{BaseUrl}/{created.LegalEntityId}",
            LegalEntityTestData.Request(
                name: "HR Agency South",
                taxId: created.TaxId,
                accounts:
                [
                    LegalEntityTestData.Account(BankAccountPurpose.Outgoing, CurrencyCode.EUR),
                ]
            )
        );

        response.EnsureSuccessStatusCode();

        var updated = await response.ReadWithJson<LegalEntityUpdated>(OutputHelper);

        Assert.NotNull(updated);
        Assert.Equal("HR Agency South", updated.Name);
        Assert.Equal(BankAccountPurpose.Outgoing, Assert.Single(updated.BankAccounts).Purpose);
    }

    [Fact]
    public async Task Closing_an_entity_records_its_last_day()
    {
        var organizationId = Guid.NewGuid();
        var created = await Create(organizationId, LegalEntityTestData.Request());

        var closed = await Close(organizationId, created.LegalEntityId, new DateOnly(2026, 6, 30));

        Assert.Equal(new DateOnly(2026, 6, 30), closed.ActiveTo);

        await Eventually.AssertAsync(async () =>
        {
            var entity = await Get(organizationId, created.LegalEntityId);

            Assert.Equal(new DateOnly(2026, 6, 30), entity.ActiveTo);
        });
    }

    [Fact]
    public async Task An_entity_cannot_be_closed_twice()
    {
        var organizationId = Guid.NewGuid();
        var created = await Create(organizationId, LegalEntityTestData.Request());

        await Close(organizationId, created.LegalEntityId, new DateOnly(2026, 6, 30));

        Client.WithOrganizationId(organizationId);

        var again = await Client.PutAsJsonAsync(
            $"{BaseUrl}/{created.LegalEntityId}/close",
            new CloseLegalEntityRequest(new DateOnly(2026, 7, 31))
        );

        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
    }

    [Fact]
    public async Task An_entity_cannot_stop_trading_before_it_started()
    {
        var organizationId = Guid.NewGuid();

        var created = await Create(
            organizationId,
            LegalEntityTestData.Request(activeFrom: new DateOnly(2026, 5, 1))
        );

        Client.WithOrganizationId(organizationId);

        var response = await Client.PutAsJsonAsync(
            $"{BaseUrl}/{created.LegalEntityId}/close",
            new CloseLegalEntityRequest(new DateOnly(2026, 4, 30))
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// The list answers "who can take a project today", so an entity that has stopped trading drops
    /// out of it without anybody running anything.
    /// </summary>
    [Fact]
    public async Task The_active_only_list_leaves_out_what_no_longer_trades()
    {
        var organizationId = Guid.NewGuid();

        var trading = await Create(organizationId, LegalEntityTestData.Request(name: "Still here"));

        var gone = await Create(
            organizationId,
            LegalEntityTestData.Request(
                name: "Wound up",
                activeFrom: new DateOnly(2019, 1, 1),
                activeTo: new DateOnly(2020, 1, 1)
            )
        );

        await Eventually.AssertAsync(async () =>
        {
            var all = await Slice(organizationId, activeOnly: false);
            var active = await Slice(organizationId, activeOnly: true);

            Assert.Contains(all.Content, z => z.Id == gone.LegalEntityId);
            Assert.Contains(active.Content, z => z.Id == trading.LegalEntityId);
            Assert.DoesNotContain(active.Content, z => z.Id == gone.LegalEntityId);
        });
    }

    [Fact]
    public async Task An_entity_of_another_organization_is_simply_not_there()
    {
        var owner = Guid.NewGuid();
        var created = await Create(owner, LegalEntityTestData.Request());

        Client.WithOrganizationId(Guid.NewGuid());

        var response = await Client.GetAsync($"{BaseUrl}/{created.LegalEntityId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<LegalEntityCreated> Create(Guid organizationId, LegalEntityRequest request)
    {
        Client.WithOrganizationId(organizationId);

        var response = await Client.PostAsJsonAsync(BaseUrl, request);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<LegalEntityCreated>(OutputHelper);

        Assert.NotNull(result);

        return result;
    }

    private async Task<LegalEntityClosed> Close(
        Guid organizationId,
        Guid legalEntityId,
        DateOnly activeTo
    )
    {
        Client.WithOrganizationId(organizationId);

        var response = await Client.PutAsJsonAsync(
            $"{BaseUrl}/{legalEntityId}/close",
            new CloseLegalEntityRequest(activeTo)
        );

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<LegalEntityClosed>(OutputHelper);

        Assert.NotNull(result);

        return result;
    }

    private async Task<LegalEntityProjection> Get(Guid organizationId, Guid legalEntityId)
    {
        Client.WithOrganizationId(organizationId);

        var response = await Client.GetAsync($"{BaseUrl}/{legalEntityId}");

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<LegalEntityProjection>(OutputHelper);

        Assert.NotNull(result);

        return result;
    }

    private async Task<SliceResponse<LegalEntityProjection>> Slice(
        Guid organizationId,
        bool activeOnly
    )
    {
        Client.WithOrganizationId(organizationId);

        var response = await Client.GetAsync($"{BaseUrl}?activeOnly={activeOnly}&pageSize=50");

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<SliceResponse<LegalEntityProjection>>(
            OutputHelper
        );

        Assert.NotNull(result);

        return result;
    }
}
