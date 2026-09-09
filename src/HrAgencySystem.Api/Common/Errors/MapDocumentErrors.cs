using HrAgencySystem.Company.Application.Create;
using HrAgencySystem.Company.Infrastructure.Persistence;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Infrastructure.Persistence;

namespace HrAgencySystem.Api.Common.Errors;

public sealed record DocumentMap(string Name, string Title, string Details);

public static class MapDocumentErrors
{
    private static readonly IReadOnlyCollection<DocumentMap> Maps =
    [
        new(nameof(CompanyTaxIdReservation), "Business rule",
            CreateCompanyHandler.TaxIdAlreadyExistsMessage),
        new(nameof(UserEmailReservation), "Business rule",
            CreateUserHandler.UserWithEmailMessage)
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