using HrAgencySystem.Forms.Domain.Values;

namespace HrAgencySystem.Api.Endpoints.FormResponses.Maps;

/// <summary>
/// The whole current set of answers, named by field code. A 400 carries fieldErrors keyed by the
/// same codes.
/// </summary>
internal sealed record AnswersRequest(IReadOnlyList<FieldAnswer> Answers);
