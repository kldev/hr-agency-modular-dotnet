using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.Values;
using FormsMaps = HrAgencySystem.Api.Endpoints.Forms.Maps;

namespace HrAgencySystem.IntegrationTests.Forms;

internal static class FormsTestData
{
    public static FormsMaps.MapCreate.CreateFormRequest CreateRequest(
        string? code = null,
        FormKind kind = FormKind.Document,
        ResponseCardinality? cardinality = null
    ) => new(code ?? $"form-{Guid.NewGuid():N}"[..16], "GDPR consent", null, kind, cardinality, null);

    public static FormField Own(
        string code,
        FieldType type = FieldType.Text,
        FieldRules? rules = null,
        IReadOnlyList<ChoiceOption>? options = null
    ) => new(Guid.NewGuid(), FieldSource.Form, null, code, type, code, null, null, null, rules ?? FieldRules.None, options ?? [], null, null);

    public static FormField FromCatalogue(Guid systemFieldId, string? labelOverride = null) =>
        new(Guid.NewGuid(), FieldSource.System, systemFieldId, "", FieldType.Text, "", labelOverride, null, null, FieldRules.None, [], null, null);

    public static FormPage Page(string title, params FormField[] fields) => new(Guid.NewGuid(), title, null, fields);

    public static FormField Consent() => Own("gdpr.consent", FieldType.Boolean, new FieldRules(Required: true));

    public static FieldAnswer Text(string code, string text) => new(code, FieldValue.OfText(text));

    public static FieldAnswer Consented(bool value = true) => new("gdpr.consent", new FieldValue(Boolean: value));
}
