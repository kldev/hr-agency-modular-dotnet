using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Infrastructure.IAM;
using Microsoft.AspNetCore.Authorization;

namespace HrAgencySystem.Api.Auth;

/// <summary>
/// Designing what the agency asks its people: forms, their publication and archiving, the catalogue
/// of system fields - and correcting a response once it has been submitted, since a submitted
/// consent is a document and amending one is not a recruiter's call.
/// <para>
/// Filling a form in and submitting it is deliberately <em>not</em> here: anybody in the organization
/// who works with a worker's file may do that, the same people who edit the file itself.
/// </para>
/// </summary>
public static class FormsDesignPolicy
{
    public const string Name = "FormsDesign";

    private static readonly OrganizationRole[] Allowed =
    [
        OrganizationRole.Admin,
        OrganizationRole.HumanResources,
    ];

    public static bool IsFormsDesigner(OrganizationRole role) => Allowed.Contains(role);

    public static void AddFormsDesignPolicy(this AuthorizationBuilder builder) =>
        builder.AddPolicy(
            Name,
            policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AppClaims.Role, [.. Allowed.Select(role => role.ToString())])
        );
}
