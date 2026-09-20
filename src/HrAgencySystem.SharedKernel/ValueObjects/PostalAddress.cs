using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

/// <summary>
/// A postal address, good enough to put on a contract or a posting declaration.
/// <para>
/// The postal code is deliberately not pattern checked. Belgium writes 1000, Germany 10115 and
/// Poland 00-001; a regex here would reject a correct address the day somebody adds a country.
/// </para>
/// </summary>
public sealed record PostalAddress
{
    public const int StreetMaxLength = 200;
    public const int BuildingNumberMaxLength = 20;
    public const int UnitNumberMaxLength = 20;
    public const int PostalCodeMaxLength = 20;
    public const int CityMaxLength = 120;

    public const string StreetRequiredMessage = "Street is required.";
    public const string StreetMaxLengthMessage = "Street cannot exceed 200 characters.";
    public const string BuildingNumberRequiredMessage = "Building number is required.";
    public const string BuildingNumberMaxLengthMessage =
        "Building number cannot exceed 20 characters.";
    public const string UnitNumberMaxLengthMessage = "Unit number cannot exceed 20 characters.";
    public const string PostalCodeRequiredMessage = "Postal code is required.";
    public const string PostalCodeMaxLengthMessage = "Postal code cannot exceed 20 characters.";
    public const string CityRequiredMessage = "City is required.";
    public const string CityMaxLengthMessage = "City cannot exceed 120 characters.";

    private PostalAddress(
        string street,
        string buildingNumber,
        string? unitNumber,
        string postalCode,
        string city,
        string countryCode
    )
    {
        Street = street;
        BuildingNumber = buildingNumber;
        UnitNumber = unitNumber;
        PostalCode = postalCode;
        City = city;
        CountryCode = countryCode;
    }

    public string Street { get; }
    public string BuildingNumber { get; }
    public string? UnitNumber { get; }
    public string PostalCode { get; }
    public string City { get; }
    public string CountryCode { get; }

    public static PostalAddress Create(
        string? street,
        string? buildingNumber,
        string? unitNumber,
        string? postalCode,
        string? city,
        string? countryCode
    )
    {
        var (address, errors) = TryCreate(
            street,
            buildingNumber,
            unitNumber,
            postalCode,
            city,
            countryCode
        );

        return errors.Count > 0 ? throw new ValidationException([.. errors]) : address!;
    }

    /// <summary>
    /// Returns every problem rather than the first one. Six fields fail together often enough that
    /// handing back one error at a time would mean six round trips to fill in one address.
    /// </summary>
    public static (PostalAddress? value, IReadOnlyList<string> errors) TryCreate(
        string? street,
        string? buildingNumber,
        string? unitNumber,
        string? postalCode,
        string? city,
        string? countryCode
    )
    {
        var errors = new List<string>();

        var streetValue = Required(
            street,
            StreetMaxLength,
            StreetRequiredMessage,
            StreetMaxLengthMessage,
            errors
        );
        var buildingValue = Required(
            buildingNumber,
            BuildingNumberMaxLength,
            BuildingNumberRequiredMessage,
            BuildingNumberMaxLengthMessage,
            errors
        );
        var postalCodeValue = Required(
            postalCode,
            PostalCodeMaxLength,
            PostalCodeRequiredMessage,
            PostalCodeMaxLengthMessage,
            errors
        );
        var cityValue = Required(
            city,
            CityMaxLength,
            CityRequiredMessage,
            CityMaxLengthMessage,
            errors
        );

        var unitValue = unitNumber?.Trim();
        if (unitValue is { Length: > UnitNumberMaxLength })
            errors.Add(UnitNumberMaxLengthMessage);

        // Fully qualified: the CountryCode property below shadows the type inside this record.
        var (country, countryError) =
            HrAgencySystem.SharedKernel.ValueObjects.CountryCode.TryCreate(countryCode ?? "");
        if (countryError is not null)
            errors.Add(countryError);

        if (errors.Count > 0)
            return (null, errors);

        return (
            new PostalAddress(
                streetValue!,
                buildingValue!,
                string.IsNullOrWhiteSpace(unitValue) ? null : unitValue,
                postalCodeValue!,
                cityValue!,
                country!.Value
            ),
            []
        );
    }

    public override string ToString()
    {
        var building = UnitNumber is null
            ? BuildingNumber
            : $"{BuildingNumber}/{UnitNumber}";

        return $"{Street} {building}, {PostalCode} {City}, {CountryCode}";
    }

    private static string? Required(
        string? input,
        int maxLength,
        string requiredMessage,
        string maxLengthMessage,
        List<string> errors
    )
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            errors.Add(requiredMessage);
            return null;
        }

        var normalized = input.Trim();

        if (normalized.Length > maxLength)
        {
            errors.Add(maxLengthMessage);
            return null;
        }

        return normalized;
    }
}
