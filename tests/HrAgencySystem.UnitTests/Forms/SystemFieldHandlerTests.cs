using HrAgencySystem.Forms.Application.SystemFields;
using HrAgencySystem.Forms.Application.SystemFields.Define;
using HrAgencySystem.Forms.Application.SystemFields.Update;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Exception;
using static HrAgencySystem.UnitTests.Forms.FormScenario;

namespace HrAgencySystem.UnitTests.Forms;

public class SystemFieldHandlerTests : BaseTest
{
    [Fact]
    public void Define_CodeOutsideTheCatalogueNamespace_IsRefused()
    {
        var error = Assert.Throws<ValidationException>(() =>
            DefineSystemFieldHandler.Prepare(Define("gdpr.consent"), Catalogue_()));

        Assert.Contains(SystemFieldRules.CodeNotSystemMessage, error.Errors);
    }

    [Fact]
    public void Define_CodeAlreadyInTheCatalogue_IsRefused()
    {
        var error = Assert.Throws<ValidationException>(() =>
            DefineSystemFieldHandler.Prepare(Define("employee.pesel"), Catalogue_()));

        Assert.Contains(SystemFieldRules.CodeTakenMessage, error.Errors);
    }

    [Fact]
    public void Define_SourceOfAnotherType_IsRefused()
    {
        var error = Assert.Throws<ValidationException>(() =>
            DefineSystemFieldHandler.Prepare(
                Define("employee.birthDate") with { Source = SystemFieldSource.WorkerDateOfBirth },
                Catalogue_()
            ));

        Assert.Contains(SystemFieldRules.SourceTypeMismatchMessage, error.Errors);
    }

    [Fact]
    public async Task Update_KeepsTheTypeAndChecksRulesAgainstIt()
    {
        // Text rules on a phone field are fine; number rules are not - the type is the catalogue's.
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateSystemFieldHandler.Handle(
                new UpdateSystemField(OrganizationId, PhoneFieldId, "Phone", null, new FieldRules(Min: 1), [], SystemFieldSource.WorkerPhone, User.Id),
                Catalogue_(),
                Service(),
                TestClock,
                CancellationToken.None
            ));

        Assert.Contains(FieldRulesPolicy.RangeNotForTypeMessage, error.Errors);
    }

    [Fact]
    public async Task Update_ArchivedField_Refuses()
    {
        var catalogue = Catalogue_();
        catalogue.Apply(new SystemFieldArchived(OrganizationId, PeselFieldId, User, Yesterday));

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            UpdateSystemFieldHandler.Handle(
                new UpdateSystemField(OrganizationId, PeselFieldId, "PESEL", null, null, null, SystemFieldSource.None, User.Id),
                catalogue,
                Service(),
                TestClock,
                CancellationToken.None
            ));

        Assert.Equal(SystemFieldRules.ArchivedMessage, error.Message);
    }

    [Fact]
    public void Catalogue_ArchivedCodeStaysTaken()
    {
        var catalogue = Catalogue_();
        catalogue.Apply(new SystemFieldArchived(OrganizationId, PeselFieldId, User, Yesterday));

        Assert.True(catalogue.HasCode("employee.pesel"));
    }

    private static DefineSystemField Define(string code) =>
        new(OrganizationId, code, FieldType.Text, "Field", null, null, null, SystemFieldSource.None, User.Id);

    private static SystemFieldCatalogue Catalogue_()
    {
        var catalogue = SystemFieldCatalogue.Empty();

        foreach (var field in Catalogue)
            catalogue.Apply(new SystemFieldDefined(OrganizationId, field, User, Yesterday));

        return catalogue;
    }
}
