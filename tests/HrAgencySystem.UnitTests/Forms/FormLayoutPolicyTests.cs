using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Domain.ValueObjects;
using HrAgencySystem.Forms.Domain.Values;
using static HrAgencySystem.UnitTests.Forms.FormScenario;

namespace HrAgencySystem.UnitTests.Forms;

public class FormLayoutPolicyTests
{
    [Fact]
    public void PrepareDraft_FillsASystemFieldFromTheCatalogue()
    {
        var (pages, errors) = FormLayoutPolicy.PrepareDraft([Page(FromCatalogue(PeselFieldId))], Catalogue);

        Assert.Empty(errors);
        var field = pages.Single().Fields.Single();
        Assert.Equal("employee.pesel", field.Code);
        Assert.Equal("PESEL", field.Label);
        Assert.Equal(StandardSystemFields.PeselPattern, field.Rules.Pattern);
    }

    [Fact]
    public void PrepareDraft_KeepsTheFormsOwnLabelForASystemField()
    {
        var (pages, _) = FormLayoutPolicy.PrepareDraft(
            [Page(FromCatalogue(PeselFieldId, "PESEL (as in the ID card)"))],
            Catalogue
        );

        var field = pages.Single().Fields.Single();
        Assert.Equal("PESEL (as in the ID card)", field.Label);
        Assert.Equal("PESEL (as in the ID card)", field.LabelOverride);
    }

    [Fact]
    public void PrepareDraft_DoesNotTrustWhatTheBuilderSaysAboutASystemField()
    {
        // The builder sends a type and rules too; the catalogue's win, or a form could loosen PESEL.
        var sent = FromCatalogue(PeselFieldId) with { Type = FieldType.Number, Rules = FieldRules.None };

        var (pages, _) = FormLayoutPolicy.PrepareDraft([Page(sent)], Catalogue);

        Assert.Equal(FieldType.Text, pages.Single().Fields.Single().Type);
        Assert.Equal(StandardSystemFields.PeselPattern, pages.Single().Fields.Single().Rules.Pattern);
    }

    [Fact]
    public void PrepareDraft_UnknownSystemField_IsMarkedOnTheField()
    {
        var field = FromCatalogue(Guid.NewGuid());

        var (_, errors) = FormLayoutPolicy.PrepareDraft([Page(field)], Catalogue);

        var error = Assert.Single(errors);
        Assert.Equal(field.FieldId, error.Target);
        Assert.Equal(FormLayoutPolicy.UnknownSystemFieldMessage, error.Message);
    }

    [Fact]
    public void PrepareDraft_ArchivedSystemField_IsRefused()
    {
        var archived = Pesel with { IsArchived = true };

        var (_, errors) = FormLayoutPolicy.PrepareDraft([Page(FromCatalogue(PeselFieldId))], [archived]);

        Assert.Contains(errors, e => e.Message == FormLayoutPolicy.ArchivedSystemFieldMessage);
    }

    [Fact]
    public void PrepareDraft_TwoFieldsWithOneCode_MarksBoth()
    {
        var first = Own("tax.office");
        var second = Own("tax.office");

        var (_, errors) = FormLayoutPolicy.PrepareDraft([Page(first), Page(second)], Catalogue);

        Assert.Equal(
            [first.FieldId, second.FieldId],
            errors.Where(e => e.Message == FormLayoutPolicy.DuplicateCodeMessage).Select(e => e.Target!.Value)
        );
    }

    [Fact]
    public void PrepareDraft_AFormFieldCannotTakeTheCatalogueNamespace()
    {
        var (_, errors) = FormLayoutPolicy.PrepareDraft([Page(Own("employee.pesel"))], Catalogue);

        Assert.Contains(errors, e => e.Message == FieldCode.ReservedPrefixMessage);
    }

    [Fact]
    public void PrepareDraft_BadCode_IsRefused()
    {
        var (_, errors) = FormLayoutPolicy.PrepareDraft([Page(Own("Tax Office"))], Catalogue);

        Assert.Contains(errors, e => e.Message == FieldCode.FormatMessage);
    }

    [Fact]
    public void PrepareDraft_RuleTheTypeCannotUse_IsRefused()
    {
        var (_, errors) = FormLayoutPolicy.PrepareDraft(
            [Page(Own("car.owned", FieldType.Boolean, new FieldRules(MaxLength: 5)))],
            Catalogue
        );

        Assert.Contains(errors, e => e.Message == FieldRulesPolicy.LengthNotForTypeMessage);
    }

    [Fact]
    public void PrepareDraft_InvalidPattern_IsRefused()
    {
        var (_, errors) = FormLayoutPolicy.PrepareDraft(
            [Page(Own("tax.number", rules: new FieldRules(Pattern: "(\\d")))],
            Catalogue
        );

        Assert.Contains(errors, e => e.Message == FieldRulesPolicy.InvalidPatternMessage);
    }

    [Fact]
    public void PrepareDraft_OptionsOfATypeWithoutOptions_AreDropped()
    {
        var field = Own("tax.office", options: [new ChoiceOption("a", "A")]);

        var (pages, errors) = FormLayoutPolicy.PrepareDraft([Page(field)], Catalogue);

        Assert.Empty(errors);
        Assert.Empty(pages.Single().Fields.Single().Options);
    }

    [Fact]
    public void PrepareDraft_DuplicateOptionValues_AreRefused()
    {
        var field = Own("tax.form", FieldType.SingleChoice, options: [new("pit", "PIT"), new("pit", "PIT-2")]);

        var (_, errors) = FormLayoutPolicy.PrepareDraft([Page(field)], Catalogue);

        Assert.Contains(errors, e => e.Message == FieldRulesPolicy.DuplicateOptionMessage);
    }

    [Fact]
    public void PrepareDraft_DefaultValueTheFieldWouldRefuse_IsRefused()
    {
        var field = Own("hours", FieldType.Number, new FieldRules(Max: 40)) with
        {
            DefaultValue = new FieldValue(Number: 50),
        };

        var (_, errors) = FormLayoutPolicy.PrepareDraft([Page(field)], Catalogue);

        Assert.Contains(errors, e => e.Target == field.FieldId && e.Message.StartsWith("Default value"));
    }

    [Fact]
    public void PrepareDraft_ConditionalFields_AreNotAcceptedYet()
    {
        var field = Own("car.make") with
        {
            VisibleWhen = new FieldVisibility("car.owned", VisibilityOperator.Equals, new FieldValue(Boolean: true)),
        };

        var (_, errors) = FormLayoutPolicy.PrepareDraft([Page(field)], Catalogue);

        Assert.Contains(errors, e => e.Message == FormLayoutPolicy.VisibilityNotSupportedMessage);
    }

    [Fact]
    public void PrepareDraft_AnUnfinishedDraft_IsFine()
    {
        // A page with nothing on it and a choice with no options yet are what a draft looks like
        // halfway through building.
        var (_, errors) = FormLayoutPolicy.PrepareDraft(
            [Page(), Page(Own("tax.form", FieldType.SingleChoice))],
            Catalogue
        );

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidatePublish_AsksForPagesFieldsAndOptions()
    {
        Assert.Contains(FormLayoutPolicy.ValidatePublish([]), e => e.Message == FormLayoutPolicy.NoPagesMessage);

        var errors = FormLayoutPolicy.ValidatePublish([Page(), Page(Own("tax.form", FieldType.SingleChoice))]);

        Assert.Contains(errors, e => e.Message == FormLayoutPolicy.EmptyPageMessage);
        Assert.Contains(errors, e => e.Message == FormLayoutPolicy.NoOptionsMessage);
    }
}
