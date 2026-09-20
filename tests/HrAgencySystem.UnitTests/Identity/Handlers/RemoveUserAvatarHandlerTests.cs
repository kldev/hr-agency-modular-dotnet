using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Avatar.Remove;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public class RemoveUserAvatarHandlerTests : BaseTest
{
    private readonly IUserProfileRepository _repository = Substitute.For<IUserProfileRepository>();

    private static readonly Guid OrganizationGuid = Guid.NewGuid();

    private static readonly Guid UserGuid = Guid.NewGuid();

    [Fact]
    public async Task Handle_WithAPicture_ReportsTheFileThatWasRemoved()
    {
        var fileId = Guid.NewGuid();

        _repository
            .RemoveAvatarAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fileId);

        var result = await RemoveUserAvatarHandler.Handle(
            Command(),
            _repository,
            CancellationToken.None
        );

        Assert.Equal(UserGuid, result.UserId);
        Assert.Equal(fileId, result.FileId);

        await _repository
            .Received(1)
            .RemoveAvatarAsync(
                Arg.Is<OrganizationId>(z => z.Value == OrganizationGuid),
                Arg.Is<UserId>(z => z.Value == UserGuid),
                Arg.Any<CancellationToken>()
            );
    }

    /// <summary>
    /// Nothing to remove is refused rather than shrugged off, so the endpoint never goes on to ask
    /// the file service to delete a file that was never there.
    /// </summary>
    [Fact]
    public async Task Handle_WithoutAPicture_Throws()
    {
        _repository
            .RemoveAvatarAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((Guid?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            RemoveUserAvatarHandler.Handle(Command(), _repository, CancellationToken.None)
        );

        Assert.Equal(RemoveUserAvatarHandler.NoAvatarMessage, exception.Message);
    }

    private static RemoveUserAvatar Command()
    {
        return new RemoveUserAvatar(UserGuid, OrganizationId.From(OrganizationGuid));
    }
}
