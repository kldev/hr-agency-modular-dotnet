using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Application.Suggestion;

public sealed record WorkerSuggestion(Guid Id, string FullName, WorkerStatus Status);
