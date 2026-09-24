using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Forms.Application.SystemFields.AddStandard;

/// <summary>Adds the fields of <c>StandardSystemFields</c> the catalogue does not have yet. Safe to repeat.</summary>
public sealed record AddStandardSystemFields(Guid OrganizationId, Guid CreatedBy) : ICreateCommand;

/// <summary>How many fields were added; zero when the catalogue already had all of them.</summary>
public sealed record StandardSystemFieldsAdded(int Added);
