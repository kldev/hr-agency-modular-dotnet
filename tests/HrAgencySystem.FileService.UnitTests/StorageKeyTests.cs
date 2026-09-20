using HrAgencySystem.FileService.Domain;

namespace HrAgencySystem.FileService.UnitTests;

public sealed class StorageKeyTests
{
    private static readonly Guid OrganizationId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );
    private static readonly Guid OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid FileId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void Create_PlacesTheObjectUnderTheOrganizationAndOwner()
    {
        var key = StorageKey.Create(OrganizationId, "project", OwnerId, FileId, ".pdf");

        Assert.Equal(
            "11111111111111111111111111111111/project/22222222222222222222222222222222/"
                + "33333333333333333333333333333333.pdf",
            key
        );
    }

    [Fact]
    public void Create_DoesNotUseTheNameTheUploaderChose()
    {
        var key = StorageKey.Create(OrganizationId, "project", OwnerId, FileId, ".pdf");

        // The whole point of generating the key: nothing a caller typed ends up in it, so nothing a
        // caller types can steer it at another object.
        Assert.DoesNotContain("..", key, StringComparison.Ordinal);
        Assert.EndsWith($"{FileId:N}.pdf", key, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Project")]
    [InlineData("  project  ")]
    public void NormalizeOwnerKind_AcceptsCasingAndPadding(string ownerKind)
    {
        Assert.Equal("project", StorageKey.NormalizeOwnerKind(ownerKind));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("../etc")]
    [InlineData("pro/ject")]
    [InlineData("pro ject")]
    [InlineData("project!")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void NormalizeOwnerKind_RefusesAnythingThatCouldLeaveThePrefix(string ownerKind)
    {
        var error = Assert.Throws<ArgumentException>(() =>
            StorageKey.NormalizeOwnerKind(ownerKind)
        );

        Assert.Contains(StorageKey.InvalidOwnerKindMessage, error.Message, StringComparison.Ordinal);
    }
}
