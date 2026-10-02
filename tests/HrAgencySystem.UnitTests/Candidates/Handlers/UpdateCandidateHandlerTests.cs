using HrAgencySystem.Recruitment.Application.Candidates.Update;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Candidates.Handlers;

public sealed class UpdateCandidateHandlerTests : BaseTest
{
    private static readonly Guid CandidateId = Guid.NewGuid();
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid ModifiedBy = Guid.NewGuid();

    private readonly IRecruitmentService _service = Substitute.For<IRecruitmentService>();

    public UpdateCandidateHandlerTests()
    {
        _service
            .GetUserAsync(ModifiedBy, Arg.Any<CancellationToken>())
            .Returns(new UserSnapshot(ModifiedBy, "Alice", "Wells", "alice@hr-agency.com"));
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsCandidateUpdated()
    {
        var (updated, _) = await Handle(Command());

        Assert.Equal("+48 500 600 700", updated.Phone);
        Assert.Equal("Joanna", updated.FirstName);
    }

    [Theory]
    [InlineData("phone")]
    [InlineData("firstName")]
    [InlineData("lastName")]
    public async Task Handle_WithAValueTooLong_ThrowsValidationException(string field)
    {
        var tooLong = new string('1', 200);
        var command = field switch
        {
            "phone" => Command() with { Phone = tooLong },
            "firstName" => Command() with { FirstName = tooLong },
            _ => Command() with { LastName = tooLong },
        };

        await Assert.ThrowsAsync<ValidationException>(() => Handle(command));
    }

    private static UpdateCandidate Command() =>
        new(CandidateId, OrganizationId, "+48 500 600 700", "Joanna", "Nowak", "", ModifiedBy);

    private Task<(CandidateUpdated, Wolverine.Marten.Events)> Handle(UpdateCandidate command) =>
        UpdateCandidateHandler.Handle(
            command,
            Candidate(),
            _service,
            TestClock,
            CancellationToken.None
        );

    private static Candidate Candidate()
    {
        var candidate = Recruitment.Domain.Candidates.Candidate.Empty();
        candidate.Apply(
            new CandidateCreated(
                CandidateId,
                OrganizationId,
                "joanna.nowak@example.com",
                "+48 600 000 000",
                CandidateSource.Direct,
                DateTimeOffset.UtcNow,
                null,
                null,
                "Joanna",
                "Nowak"
            )
        );

        return candidate;
    }
}
