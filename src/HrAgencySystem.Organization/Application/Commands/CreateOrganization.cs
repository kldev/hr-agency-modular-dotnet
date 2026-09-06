using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Organization.Application.Commands;

public sealed record CreateOrganization(string Name, string Slug, Guid CreatedBy, IReadOnlyList<string> EmailDomains): ICreateCommand;