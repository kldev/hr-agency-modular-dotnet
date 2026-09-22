namespace HrAgencySystem.Workers.Contracts.IntegrationEvents;

/// <summary>
/// A file was opened for somebody who came through recruitment. Sent so the candidate - and the
/// application the decision was made on, when there was one - can say that this person is now in
/// the register.
/// <para>
/// This module cannot see either record, so the ids are claims rather than facts: the receiving
/// side checks that they exist, belong to the same organization and belong to each other, and
/// ignores the message when they do not.
/// </para>
/// </summary>
public sealed record WorkerRegisteredFromRecruitment(
    Guid OrganizationId,
    Guid WorkerId,
    Guid CandidateId,
    Guid? ApplicationId
);
