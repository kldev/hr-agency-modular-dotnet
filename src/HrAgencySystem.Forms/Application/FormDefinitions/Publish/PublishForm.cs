using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Forms.Application.FormDefinitions.Publish;

public sealed record PublishForm([property: Identity] Guid FormId, Guid OrganizationId, Guid ModifiedBy)
    : IUpdateCommand;
