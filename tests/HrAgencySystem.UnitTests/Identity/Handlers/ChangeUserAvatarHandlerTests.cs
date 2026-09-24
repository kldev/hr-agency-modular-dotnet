using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Avatar.Change;
using HrAgencySystem.Identity.Documents;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public class ChangeUserAvatarHandlerTests : BaseTest
{
    private readonly IUserProfileRepository _repository = Substitute.For<IUserProfileRepository>();

    private static readonly Guid OrganizationGuid = Guid.NewGuid();

    private static readonly Guid UserGuid = Guid.NewGuid();

    private static readonly Guid FileGuid = Guid.NewGuid();

    [Fact]
    public async Task Handle_WithTheFirstPicture_ReportsNothingToDelete()
    {
        var now = new DateTimeOffset(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);

        _repository
            .SetAvatarAsync(Arg.Any<UserProfile>(), Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var result = await ChangeUserAvatarHandler.Handle(
            Command(),
            _repository,
            new FixedClock(now),
            CancellationToken.None
        );

        Assert.Equal(UserGuid, result.UserId);
        Assert.Equal(FileGuid, result.FileId);
        Assert.Null(result.PreviousFileId);
    }

    /// <summary>
    /// The displaced file is what the endpoint needs to clean up, so losing it here would leave an
    /// object in the bucket that nothing points at and nothing will ever look for again.
    /// </summary>
    [Fact]
    public async Task Handle_WhenReplacing_ReportsTheDisplacedFile()
    {
        var previous = Guid.NewGuid();

        _repository
            .SetAvatarAsync(Arg.Any<UserProfile>(), Arg.Any<CancellationToken>())
            .Returns(previous);

        var result = await ChangeUserAvatarHandler.Handle(
            Command(),
            _repository,
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(previous, result.PreviousFileId);
    }

    [Fact]
    public async Task Handle_StoresTheProfileForTheActingUserAndOrganization()
    {
        var now = new DateTimeOffset(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);

        await ChangeUserAvatarHandler.Handle(
            Command(),
            _repository,
            new FixedClock(now),
            CancellationToken.None
        );

        await _repository
            .Received(1)
            .SetAvatarAsync(
                Arg.Is<UserProfile>(p =>
                    p.Id == UserGuid
                    && p.OrganizationId == OrganizationGuid
                    && p.AvatarFileId == FileGuid
                    && p.AvatarFileName == "face.png"
                    && p.AvatarContentType == "image/png"
                    && p.AvatarSize == 2048
                    && p.AvatarUploadedAt == now
                ),
                Arg.Any<CancellationToken>()
            );
    }

    private static ChangeUserAvatar Command()
    {
        return new ChangeUserAvatar(
            UserGuid,
            OrganizationId.From(OrganizationGuid),
            FileGuid,
            "face.png",
            "image/png",
            2048,
            UserGuid
        );
    }
}
