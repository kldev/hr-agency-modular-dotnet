using HrAgencySystem.Compliance;

namespace HrAgencySystem.UnitTests.Compliance;

/// <summary>
/// Tests our decision about which requirements to track, not the state of the law. Every entry here
/// is backed by an official source recorded next to the enum value; anything that could not be
/// confirmed deliberately never made it into the catalogue.
/// </summary>
public class ComplianceCatalogueTests
{
    private const ComplianceScope Project = ComplianceScope.Project;
    private const ComplianceScope Assignment = ComplianceScope.Assignment;

    [Fact]
    public void Belgium_TemporaryAgencyWork_NeedsRecognitionAndBothJointCommittees()
    {
        var requirements = ComplianceCatalogue.For(
            "BE",
            EngagementType.TemporaryAgencyWork,
            Project
        );

        Assert.Contains(ComplianceRequirement.BeTemporaryAgencyRecognition, requirements);
        Assert.Contains(ComplianceRequirement.BeLiaisonPerson, requirements);

        // Two committees, not one: ours covers the activity, the user's sets the pay. One field for
        // both would quietly hide the second.
        Assert.Contains(ComplianceRequirement.BeJointCommittee, requirements);
        Assert.Contains(ComplianceRequirement.BeUserJointCommittee, requirements);
    }

    [Fact]
    public void Belgium_Posting_DoesNotNeedAgencyRecognitionOrTheUsersCommittee()
    {
        var requirements = ComplianceCatalogue.For("BE", EngagementType.PostingOfWorkers, Project);

        Assert.DoesNotContain(ComplianceRequirement.BeTemporaryAgencyRecognition, requirements);
        Assert.DoesNotContain(ComplianceRequirement.BeUserJointCommittee, requirements);
    }

    [Fact]
    public void Germany_HiringOut_CarriesTheLicenceAndItsNotification()
    {
        var requirements = ComplianceCatalogue.For(
            "DE",
            EngagementType.TemporaryAgencyWork,
            Project
        );

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
        var requirements = ComplianceCatalogue.For("DE", EngagementType.PostingOfWorkers, Project);

        Assert.DoesNotContain(ComplianceRequirement.DeAuegPermit, requirements);
        Assert.DoesNotContain(ComplianceRequirement.DeAuegNotification, requirements);
        Assert.Contains(ComplianceRequirement.DeAentgNotification, requirements);
    }

    [Theory]
    [InlineData(EngagementType.PostingOfWorkers)]
    [InlineData(EngagementType.TemporaryAgencyWork)]
    [InlineData(EngagementType.Outsourcing)]
    public void EveryPostingCarriesTheEuWideRequirements(EngagementType engagement)
    {
        foreach (var country in new[] { "BE", "DE" })
            Assert.Contains(
                ComplianceRequirement.A1Certificates,
                ComplianceCatalogue.For(country, engagement, Assignment)
            );
    }

    /// <summary>
    /// The row the whole engagement type extension exists for. Employing somebody under the host
    /// country's own law is not a posting, so none of the posting machinery applies - and a system
    /// that asked for an A1 here would be asking for a document nobody will ever issue.
    /// </summary>
    [Theory]
    [InlineData("DE", ComplianceRequirement.DeSocialSecurityRegistration)]
    [InlineData("BE", ComplianceRequirement.BeDimonaDeclaration)]
    public void LocalEmployment_HasNoA1AndNoPostingNotification(
        string country,
        ComplianceRequirement localFiling
    )
    {
        var requirements = ComplianceCatalogue.For(
            country,
            EngagementType.LocalEmployment,
            Assignment
        );

        Assert.DoesNotContain(ComplianceRequirement.A1Certificates, requirements);
        Assert.DoesNotContain(ComplianceRequirement.BeLimosaDeclaration, requirements);
        Assert.DoesNotContain(ComplianceRequirement.DeLongTermPostingNotification, requirements);

        Assert.Contains(ComplianceRequirement.LocalEmploymentContract, requirements);
        Assert.Contains(localFiling, requirements);
    }

    [Fact]
    public void LocalEmployment_CarriesNothingAtProjectLevel()
    {
        // Everything local employment needs is about one person, so the project's own list is
        // empty. Not a gap: there is no host state notification to make when nobody was posted.
        Assert.Empty(ComplianceCatalogue.For("DE", EngagementType.LocalEmployment, Project));
        Assert.Empty(ComplianceCatalogue.For("BE", EngagementType.LocalEmployment, Project));
    }

    /// <summary>
    /// Outsourcing is a posting carried out under our own direction: the posting duties apply, plus
    /// the one that does not exist anywhere else - showing the client is not in fact directing the
    /// work, which is what would turn it into unlicensed hiring out.
    /// </summary>
    [Theory]
    [InlineData("DE", ComplianceRequirement.DeServiceContractDelimitation)]
    [InlineData("BE", ComplianceRequirement.BeProhibitedPlacement)]
    public void Outsourcing_IsPostingPlusTheDelimitationCheck(
        string country,
        ComplianceRequirement delimitation
    )
    {
        var posting = ComplianceCatalogue.For(country, EngagementType.PostingOfWorkers, Project);
        var outsourcing = ComplianceCatalogue.For(country, EngagementType.Outsourcing, Project);

        Assert.All(posting, requirement => Assert.Contains(requirement, outsourcing));
        Assert.Contains(delimitation, outsourcing);
        Assert.DoesNotContain(delimitation, posting);
    }

    [Theory]
    [InlineData(EngagementType.PostingOfWorkers)]
    [InlineData(EngagementType.TemporaryAgencyWork)]
    [InlineData(EngagementType.Outsourcing)]
    [InlineData(EngagementType.LocalEmployment)]
    public void Poland_HasNothingToTrack(EngagementType engagement)
    {
        // An empty catalogue is the correct answer rather than missing data: working in the country
        // of establishment does not raise a host state's obligations.
        Assert.Empty(ComplianceCatalogue.For("PL", engagement, Project));
        Assert.Empty(ComplianceCatalogue.For("PL", engagement, Assignment));
    }

    [Fact]
    public void AnUnknownCountryIsEmptyRatherThanAFailure()
    {
        Assert.Empty(ComplianceCatalogue.For("FR", EngagementType.PostingOfWorkers, Project));
        Assert.Empty(ComplianceCatalogue.For("", EngagementType.PostingOfWorkers, Project));
    }

    [Theory]
    [InlineData("be")]
    [InlineData("  BE  ")]
    public void TheCountryKeyIsCaseAndPaddingInsensitive(string country)
    {
        Assert.NotEmpty(ComplianceCatalogue.For(country, EngagementType.PostingOfWorkers, Project));
    }

    [Fact]
    public void ContainsAnswersWhetherARequirementBelongsAtThatLevel()
    {
        Assert.True(
            ComplianceCatalogue.Contains(
                "DE",
                EngagementType.TemporaryAgencyWork,
                Project,
                ComplianceRequirement.DeAuegPermit
            )
        );
        Assert.False(
            ComplianceCatalogue.Contains(
                "DE",
                EngagementType.PostingOfWorkers,
                Project,
                ComplianceRequirement.DeAuegPermit
            )
        );

        // Right country, right engagement, wrong level: the A1 is one person's, so the project's
        // list does not have it and the assignment's does.
        Assert.False(
            ComplianceCatalogue.Contains(
                "DE",
                EngagementType.PostingOfWorkers,
                Project,
                ComplianceRequirement.A1Certificates
            )
        );
        Assert.True(
            ComplianceCatalogue.Contains(
                "DE",
                EngagementType.PostingOfWorkers,
                Assignment,
                ComplianceRequirement.A1Certificates
            )
        );
    }

    /// <summary>
    /// The gap CLAUDE.md reported, closed. A1 is issued to a named person for a named period, so it
    /// cannot be recorded as one tick covering everybody on a project.
    /// </summary>
    [Fact]
    public void PerPersonRequirementsAreScopedToTheAssignment()
    {
        Assert.Equal(Assignment, ComplianceCatalogue.ScopeOf(ComplianceRequirement.A1Certificates));
        Assert.Equal(
            Assignment,
            ComplianceCatalogue.ScopeOf(ComplianceRequirement.BeLimosaDeclaration)
        );
        Assert.Equal(
            Assignment,
            ComplianceCatalogue.ScopeOf(ComplianceRequirement.LocalEmploymentContract)
        );

        // Held by the company for everybody it posts, so it stays where it was.
        Assert.Equal(Project, ComplianceCatalogue.ScopeOf(ComplianceRequirement.DeAuegPermit));
        Assert.Equal(
            Project,
            ComplianceCatalogue.ScopeOf(ComplianceRequirement.DeAuthorisedRecipient)
        );
    }

    [Fact]
    public void TheTwoScopesTogetherAreTheWholeCatalogueAndNeverOverlap()
    {
        foreach (var country in new[] { "BE", "DE" })
        foreach (var engagement in Enum.GetValues<EngagementType>())
        {
            var project = ComplianceCatalogue.For(country, engagement, Project);
            var assignment = ComplianceCatalogue.For(country, engagement, Assignment);

            Assert.Empty(project.Intersect(assignment));
            Assert.Equal(project.Count, project.Distinct().Count());
            Assert.Equal(assignment.Count, assignment.Distinct().Count());
        }
    }
}
