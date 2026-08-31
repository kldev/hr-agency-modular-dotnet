using HrAgencySystem.Company.Application.Handlers;
using HrAgencySystem.Company.Infrastructure.Persistence;

namespace HrAgencySystem.Api.Common.Errors;

public sealed record DocumentMap(string Name, string Title, string Details);

public static class MapDocumentErrors
{
    private static readonly IReadOnlyCollection<DocumentMap> Maps =
    [
        new(nameof(CompanyTaxIdReservation), "Company tax ID already exists",
            CreateCompanyHandler.TaxIdAlreadyExistsMessage)
    ];

    public static string Title(string name)
    {
        return Maps.SingleOrDefault(x => x.Name.Contains(name))?.Title ?? "Document already exits";
    }

    public static string Details(string name)
    {
        return Maps.SingleOrDefault(x => x.Name.Contains(name))?.Details ??
               "Data with provided request is already stored in database";
    }
}