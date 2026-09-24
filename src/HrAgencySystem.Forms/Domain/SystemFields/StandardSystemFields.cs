using HrAgencySystem.Forms.Domain.Layout;

namespace HrAgencySystem.Forms.Domain.SystemFields;

/// <summary>
/// The fields an agency asks for in almost every document, offered as one click in the catalogue
/// ("add standard fields") instead of being planted into every new organization. The six with a
/// source fill themselves from the worker's file; the rest are the administrator's to keep.
/// </summary>
public static class StandardSystemFields
{
    public sealed record Definition(
        string Code,
        FieldType Type,
        string Label,
        string? Description,
        FieldRules Rules,
        SystemFieldSource Source
    );

    public const string PeselPattern = "\\d{11}";

    public static readonly IReadOnlyList<Definition> All =
    [
        new("employee.firstName", FieldType.Text, "First name", null, new(MaxLength: 100), SystemFieldSource.WorkerFirstName),
        new("employee.lastName", FieldType.Text, "Last name", null, new(MaxLength: 100), SystemFieldSource.WorkerLastName),
        new("employee.dateOfBirth", FieldType.Date, "Date of birth", null, FieldRules.None, SystemFieldSource.WorkerDateOfBirth),
        new("employee.citizenship", FieldType.Country, "Citizenship", null, FieldRules.None, SystemFieldSource.WorkerCitizenship),
        new("employee.email", FieldType.Email, "E-mail", null, FieldRules.None, SystemFieldSource.WorkerEmail),
        new("employee.phone", FieldType.Phone, "Phone", null, FieldRules.None, SystemFieldSource.WorkerPhone),
        new(
            "employee.pesel",
            FieldType.Text,
            "PESEL",
            "Polish national identification number.",
            new(Pattern: PeselPattern, Message: "Enter a valid PESEL number (11 digits)."),
            SystemFieldSource.None
        ),
        new(
            "employee.bankAccount",
            FieldType.Text,
            "Bank account number",
            "IBAN of the account salary is paid to.",
            new(MaxLength: 34),
            SystemFieldSource.None
        ),
    ];
}
