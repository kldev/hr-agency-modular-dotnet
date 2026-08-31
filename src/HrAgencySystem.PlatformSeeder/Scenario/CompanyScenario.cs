using Bogus;
using HrAgencySystem.Company.Application.Commands;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class CompanyScenario(IMessageBus bus)
{
    private static readonly CountryDefinition[] Countries =
    [
        new("PL", "PL", "NIP"),
        new("BE", "BE", "VAT"),
        new("DE", "DE", "VAT"),
        new("NL", "NL", "VAT"),
        new("FR", "FR", "VAT"),
        new("AT", "AT", "VAT"),
        new("CZ", "CZ", "VAT"),
        new("SK", "SK", "VAT"),
        new("DK", "DK", "VAT"),
        new("SE", "SE", "VAT"),
        new("NO", "NO", "VAT"),
        new("FI", "FI", "VAT"),
        new("IT", "IT", "VAT"),
        new("ES", "ES", "VAT"),
        new("PT", "PT", "VAT"),
        new("IE", "IE", "VAT"),
        new("RO", "RO", "VAT"),
        new("HU", "HU", "VAT")
    ];

    internal async Task Create(Guid organizationId, int seedCount = 101)
    {
        var faker = new Faker();

        var companies = Enumerable
            .Range(1, seedCount)
            .Select(index =>
            {
                var country = faker.PickRandom(Countries);

                return new CreateCompany(
                    organizationId,
                    GenerateCompanyName(country, index),
                    country.Code,
                    GenerateTaxId(faker, country),
                    GenerateRegistrationNumber(faker, country));
            });

        foreach (var company in companies)
        {
            await bus.InvokeAsync(company);
        }
    }

    private static string GenerateCompanyName(
        CountryDefinition country,
        int index)
    {
        var name = country.Code switch
        {
            //https://github.com/bchavez/Bogus#locales
            "DE" => new Bogus.DataSets.Company(locale: "de").CompanyName(),
            "FR" => new Bogus.DataSets.Company(locale: "fr").CompanyName(),
            "ES" => new Bogus.DataSets.Company(locale: "es").CompanyName(),
            _ => new Bogus.DataSets.Company(locale: "en_US").CompanyName(),
        };

        return $"{name} {index:000}";
    }

    private static string GenerateTaxId(
        Faker faker,
        CountryDefinition country)
    {
        return country.Code switch
        {
            "PL" => faker.Random.ReplaceNumbers("##########"),

            "BE" => faker.Random.ReplaceNumbers("BE0#########"),

            "DE" => $"DE{faker.Random.ReplaceNumbers("#########")}",

            "NL" => $"NL{faker.Random.ReplaceNumbers("#########")}B{faker.Random.Number(0, 9)}",

            "FR" => $"FR{faker.Random.AlphaNumeric(2).ToUpperInvariant()}{faker.Random.ReplaceNumbers("#########")}",

            "AT" => $"ATU{faker.Random.ReplaceNumbers("########")}",

            "CZ" => $"CZ{faker.Random.ReplaceNumbers("########")}",

            "SK" => $"SK{faker.Random.ReplaceNumbers("##########")}",

            "DK" => $"DK{faker.Random.ReplaceNumbers("########")}",

            "SE" => $"SE{faker.Random.ReplaceNumbers("##########")}01",

            "NO" => $"NO{faker.Random.ReplaceNumbers("#########")}",

            "FI" => $"FI{faker.Random.ReplaceNumbers("########")}",

            "IT" => $"IT{faker.Random.ReplaceNumbers("###########")}",

            "ES" => $"ES{faker.Random.ReplaceNumbers("########")}",

            "PT" => $"PT{faker.Random.ReplaceNumbers("#########")}",

            "IE" => $"IE{faker.Random.AlphaNumeric(1).ToUpperInvariant()}{faker.Random.ReplaceNumbers("########")}",

            "RO" => $"RO{faker.Random.ReplaceNumbers("#########")}",

            "HU" => $"HU{faker.Random.ReplaceNumbers("########")}",

            _ => faker.Random.ReplaceNumbers("############")
        };
    }

    private static string GenerateRegistrationNumber(
        Faker faker,
        CountryDefinition country)
    {
        return country.Code switch
        {
            "PL" => faker.Random.ReplaceNumbers("##########"),

            "BE" => faker.Random.ReplaceNumbers("0#########"),

            "DE" => faker.Random.ReplaceNumbers("HRB ######"),

            "NL" => faker.Random.ReplaceNumbers("########"),

            "FR" => faker.Random.ReplaceNumbers("### ### ###"),

            "AT" => faker.Random.ReplaceNumbers("#########"),

            "CZ" => faker.Random.ReplaceNumbers("########"),

            "SK" => faker.Random.ReplaceNumbers("########"),

            "DK" => faker.Random.ReplaceNumbers("########"),

            "SE" => faker.Random.ReplaceNumbers("##########"),

            "NO" => faker.Random.ReplaceNumbers("#########"),

            "FI" => faker.Random.ReplaceNumbers("########-#"),

            "IT" => faker.Random.ReplaceNumbers("###########"),

            "ES" => faker.Random.ReplaceNumbers("########"),

            "PT" => faker.Random.ReplaceNumbers("#########"),

            "IE" => faker.Random.ReplaceNumbers("########"),

            "RO" => $"J{faker.Random.Number(1, 52)}/{faker.Random.Number(100, 9999)}/{faker.Random.Number(2000, 2026)}",

            "HU" => faker.Random.ReplaceNumbers("########-#"),

            _ => faker.Random.ReplaceNumbers("##########")
        };
    }

    private sealed record CountryDefinition(
        string Code,
        string TaxPrefix,
        string TaxType);
}
