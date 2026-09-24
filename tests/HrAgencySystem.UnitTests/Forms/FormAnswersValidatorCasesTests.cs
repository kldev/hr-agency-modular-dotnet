using System.Text.Json;
using System.Text.Json.Serialization;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.Validation;
using HrAgencySystem.Forms.Domain.Values;

namespace HrAgencySystem.UnitTests.Forms;

/// <summary>
/// The validator against <c>tests/fixtures/forms-validation-cases.json</c> - the file the front end's
/// Vitest suite reads too. It is the only thing that keeps the C# rules and their TypeScript mirror
/// saying the same; a rule added without a case here is a rule nobody checks on both sides.
/// </summary>
public class FormAnswersValidatorCasesTests
{
    private const string FieldCode = "case.field";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static TheoryData<string> CaseNames() => [.. Load().Select(c => c.Name)];

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void Case_GivesTheExpectedError(string name)
    {
        var @case = Load().Single(c => c.Name == name);

        var field = new FormField(
            Guid.NewGuid(),
            FieldSource.Form,
            null,
            FieldCode,
            @case.Field.Type,
            "Field",
            null,
            null,
            null,
            @case.Field.Rules,
            @case.Field.Options,
            null,
            null
        );

        var pages = new[] { new FormPage(Guid.NewGuid(), "Page", null, [field]) };
        FieldAnswer[] answers = @case.Value is null
            ? []
            : [new FieldAnswer(FieldCode, @case.Value)];

        var errors = FormAnswersValidator.Validate(
            pages,
            FieldAnswers.Normalize(pages, answers),
            @case.Mode == "draft" ? ValidationMode.Draft : ValidationMode.Submit
        );

        Assert.Equal(@case.Expected, errors.SingleOrDefault()?.Code);

        if (@case.Field.Rules.Message is { } message && @case.Expected is not null)
            Assert.Equal(message, errors.Single().Message);
    }

    private static IReadOnlyList<ValidationCase> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Forms", "forms-validation-cases.json");

        return JsonSerializer.Deserialize<CaseFile>(File.ReadAllText(path), Json)!.Cases;
    }

    private sealed record CaseFile(IReadOnlyList<ValidationCase> Cases);

    private sealed record ValidationCase(
        string Name,
        CaseField Field,
        FieldValue? Value,
        string Mode,
        string? Expected
    );

    private sealed record CaseField(
        FieldType Type,
        FieldRules Rules,
        IReadOnlyList<ChoiceOption> Options
    );
}
