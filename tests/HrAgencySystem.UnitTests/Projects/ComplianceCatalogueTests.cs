using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Domain.Compliance;

namespace HrAgencySystem.UnitTests.Projects;

/// <summary>
/// Tests our decision about which requirements to track, not the state of the law. Every entry here
/// is backed by an official source recorded next to the enum value; anything that could not be
/// confirmed deliberately never made it into the catalogue.
/// </summary>
public class ComplianceCatalogueTests
{
    [Fact]
    public void Belgium_TemporaryAgencyWork_NeedsRecognitionAndBothJointCommittees()
    {
        var requirements = ComplianceCatalogue.For("BE", EngagementType.TemporaryAgencyWork);

        Assert.Contains(ComplianceRequirement.BeTemporaryAgencyRecognition, requirements);
        Assert.Contains(ComplianceRequirement.BeLimosaDeclaration, requirements);
        Assert.Contains(ComplianceRequirement.BeLiaisonPerson, requirements);

        // Two committees, not one: ours covers the activity, the user's sets the pay. One field for
        // both would quietly hide the second.
        Assert.Contains(ComplianceRequirement.BeJointCommittee, requirements);
        Assert.Contains(ComplianceRequirement.BeUserJointCommittee, requirements);
    }

    [Fact]
    public void Belgium_Posting_DoesNotNeedAgencyRecognitionOrTheUsersCommittee()
    {
        var requirements = ComplianceCatalogue.For("BE", EngagementType.PostingOfWorkers);

        Assert.Contains(ComplianceRequirement.BeLimosaDeclaration, requirements);
        Assert.DoesNotContain(ComplianceRequirement.BeTemporaryAgencyRecognition, requirements);
        Assert.DoesNotContain(ComplianceRequirement.BeUserJointCommittee, requirements);
    }

    [Fact]
    public void Germany_HiringOut_CarriesTheLicenceAndItsNotification()
    {
        var requirements = ComplianceCatalogue.For("DE", EngagementType.TemporaryAgencyWork);

        Assert.Contains(ComplianceRequirement.DeAuegPermit, requirements);
        Assert.Contains(ComplianceRequirement.DeAuegNotification, requirements);
        Assert.Contains(ComplianceRequirement.DeUeberlassungAgreement, requirements);
        Assert.Contains(ComplianceRequirement.DeConstructionSectorRestriction, requirements);
    }

    [Fact]
    public void Germany_Posting_CarriesNeitherTheLicenceNorItsNotification()
    {
        // The whole reason the catalogue is keyed on both country and engagement type: hiring
        // somebody out to a German user is a supervised sector in itself, posting an IT specialist
        // there is not.
        var requirements = ComplianceCatalogue.For("DE", EngagementType.PostingOfWorkers);

        Assert.DoesNotContain(ComplianceRequirement.DeAuegPermit, requirements);
        Assert.DoesNotContain(ComplianceRequirement.DeAuegNotification, requirements);
        Assert.Contains(ComplianceRequirement.DeAentgNotification, requirements);
    }

    [Theory]
    [InlineData(EngagementType.PostingOfWorkers)]
    [InlineData(EngagementType.TemporaryAgencyWork)]
    public void EveryCountryCarriesTheEuWideRequirements(EngagementType engagement)
    {
        foreach (var country in new[] { "BE", "DE" })
            Assert.Contains(
                ComplianceRequirement.A1Certificates,
                ComplianceCatalogue.For(country, engagement)
            );
    }

    [Theory]
    [InlineData(EngagementType.PostingOfWorkers)]
    [InlineData(EngagementType.TemporaryAgencyWork)]
    public void Poland_HasNothingToTrack(EngagementType engagement)
    {
        // An empty catalogue is the correct answer rather than missing data: working in the country
        // of establishment does not raise a host state's obligations.
        Assert.Empty(ComplianceCatalogue.For("PL", engagement));
    }

    [Fact]
    public void AnUnknownCountryIsEmptyRatherThanAFailure()
    {
        Assert.Empty(ComplianceCatalogue.For("FR", EngagementType.PostingOfWorkers));
        Assert.Empty(ComplianceCatalogue.For("", EngagementType.PostingOfWorkers));
    }

    [Theory]
    [InlineData("be")]
    [InlineData("  BE  ")]
    public void TheCountryKeyIsCaseAndPaddingInsensitive(string country)
    {
        Assert.NotEmpty(ComplianceCatalogue.For(country, EngagementType.PostingOfWorkers));
    }

    [Fact]
    public void ContainsAnswersWhetherARequirementBelongsToTheProject()
    {
        Assert.True(
            ComplianceCatalogue.Contains(
                "DE",
                EngagementType.TemporaryAgencyWork,
                ComplianceRequirement.DeAuegPermit
            )
        );
        Assert.False(
            ComplianceCatalogue.Contains(
                "DE",
                EngagementType.PostingOfWorkers,
                ComplianceRequirement.DeAuegPermit
            )
        );
    }

    [Fact]
    public void NoRequirementIsListedTwiceForOneProject()
    {
        foreach (var country in new[] { "BE", "DE" })
        foreach (var engagement in Enum.GetValues<EngagementType>())
        {
            var requirements = ComplianceCatalogue.For(country, engagement);

            Assert.Equal(requirements.Count, requirements.Distinct().Count());
        }
    }
}
