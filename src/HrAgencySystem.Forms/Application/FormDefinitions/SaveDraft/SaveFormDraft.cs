using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Forms.Application.FormDefinitions.SaveDraft;

/// <summary>
/// The builder's whole working layout. Whole, not change by change: the builder edits locally, the
/// preview renders what is not saved yet, and the rules worth checking - codes used once, system
/// fields that exist - are facts about the set (plan 028 §3.3).
/// <para>
/// Page and field ids are the builder's; a system field needs only its <c>SystemFieldId</c> and,
/// optionally, a <c>LabelOverride</c> - everything else about it is filled from the catalogue.
/// </para>
/// </summary>
public sealed record SaveFormDraft(
    [property: Identity] Guid FormId,
    Guid OrganizationId,
    IReadOnlyList<FormPage> Pages,
    Guid ModifiedBy
) : IUpdateCommand;
