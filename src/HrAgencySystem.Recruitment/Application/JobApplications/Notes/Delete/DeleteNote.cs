namespace HrAgencySystem.Recruitment.Application.JobApplications.Notes.Delete;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record DeleteNote(Guid JobApplicationId, Guid OrganizationId, Guid NoteId, Guid RemovedBy);