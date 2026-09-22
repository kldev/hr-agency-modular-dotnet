using HrAgencySystem.Api.Auth;
using HrAgencySystem.Identity.Application.ApiKeys.Issue;
using HrAgencySystem.Identity.Application.ApiKeys.Revoke;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text.Encodings.Web;

namespace HrAgencySystem.UnitTests.Identity;

/// <summary>
/// The key scheme on its own. The case that matters most is the revoked key: revoking is the only
/// way a leaked key stops working, so a revoked one that still authenticates would be a key that
/// can never be taken back.
/// </summary>
public class ApiKeyAuthenticationHandlerTests : BaseTest
{
    private readonly IServiceApiKeyRepository _keys = Substitute.For<IServiceApiKeyRepository>();

    [Fact]
    public async Task NoHeader_IsNoResult()
    {
        var result = await Authenticate(null);

        Assert.True(result.None);
    }

    /// <summary>A user's bearer token pasted into the key header is refused before any lookup.</summary>
    [Fact]
    public async Task AValueWithoutThePrefix_Fails_WithoutALookup()
    {
        var result = await Authenticate("eyJhbGciOiJIUzI1NiJ9.a-user-token");

        Assert.False(result.Succeeded);
        Assert.False(result.None);
        await _keys.DidNotReceive().FindByHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnUnknownKey_Fails()
    {
        var result = await Authenticate("sk_" + SecureToken.New());

        Assert.False(result.Succeeded);
        Assert.False(result.None);
    }

    [Fact]
    public async Task ARevokedKey_Fails()
    {
        var (key, value) = ServiceApiKey.Issue("public job board", Guid.NewGuid(), TestClock);
        Stored(key.Revoke(Guid.NewGuid(), TestClock));

        var result = await Authenticate(value);

        Assert.False(result.Succeeded);
        Assert.False(result.None);
    }

    [Fact]
    public async Task AValidKey_Succeeds_AndSaysWhichKeyItWas()
    {
        var (key, value) = ServiceApiKey.Issue("public job board", Guid.NewGuid(), TestClock);
        Stored(key);

        var result = await Authenticate(value);

        Assert.True(result.Succeeded);
        Assert.Equal(
            key.Id.ToString(),
            result.Principal!.FindFirst(ApiKeyAuthenticationHandler.KeyIdClaim)?.Value
        );
    }

    [Fact]
    public void Issue_StartsTheValueWithThePrefix_AndStoresOnlyTheHash()
    {
        var (key, value) = ServiceApiKey.Issue("public job board", Guid.NewGuid(), TestClock);

        Assert.StartsWith(ServiceApiKey.ValuePrefix, value);
        Assert.Equal(SecureToken.Hash(value), key.KeyHash);
        Assert.DoesNotContain(value, key.KeyHash);
        Assert.Equal(value[..11], key.DisplayPrefix);
    }

    [Fact]
    public void IssueHandler_ReturnsTheValueOnce_AndRefusesANamelessKey()
    {
        var issued = IssueServiceApiKeyHandler.Handle(
            new IssueServiceApiKey("  public job board  ", Guid.NewGuid()),
            _keys,
            TestClock
        );

        Assert.Equal("public job board", issued.Name);
        Assert.StartsWith(ServiceApiKey.ValuePrefix, issued.Value);
        _keys.Received(1).Issue(Arg.Is<ServiceApiKey>(k => k.KeyHash == SecureToken.Hash(issued.Value)));

        var error = Assert.Throws<ValidationException>(() =>
            IssueServiceApiKeyHandler.Handle(new IssueServiceApiKey(" ", Guid.NewGuid()), _keys, TestClock)
        );
        Assert.Contains(ServiceApiKey.NameRequiredMessage, error.Message);
    }

    /// <summary>Revoking twice keeps the first revocation - who did it and when.</summary>
    [Fact]
    public async Task RevokeHandler_KeepsTheFirstRevocation()
    {
        var (key, _) = ServiceApiKey.Issue("public job board", Guid.NewGuid(), TestClock);
        var revoked = key.Revoke(Guid.NewGuid(), TestClock);
        _keys.GetAsync(key.Id, Arg.Any<CancellationToken>()).Returns(revoked);

        var result = await RevokeServiceApiKeyHandler.Handle(
            new RevokeServiceApiKey(key.Id, Guid.NewGuid()),
            _keys,
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(revoked.RevokedAt, result.RevokedAt);
        _keys.DidNotReceive().Update(Arg.Any<ServiceApiKey>());
    }

    private void Stored(ServiceApiKey key) =>
        _keys.FindByHashAsync(key.KeyHash, Arg.Any<CancellationToken>()).Returns(key);

    private async Task<AuthenticateResult> Authenticate(string? headerValue)
    {
        var options = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());

        var handler = new ApiKeyAuthenticationHandler(
            options,
            NullLoggerFactory.Instance,
            UrlEncoder.Default,
            _keys
        );

        var context = new DefaultHttpContext();

        if (headerValue is not null)
            context.Request.Headers[ApiKeyAuthenticationHandler.HeaderName] = headerValue;

        await handler.InitializeAsync(
            new AuthenticationScheme(
                ApiKeyAuthenticationHandler.SchemeName,
                null,
                typeof(ApiKeyAuthenticationHandler)
            ),
            context
        );

        return await handler.AuthenticateAsync();
    }
}
