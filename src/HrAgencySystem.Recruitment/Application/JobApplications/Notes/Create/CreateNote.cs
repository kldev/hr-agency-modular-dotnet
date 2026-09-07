namespace HrAgencySystem.Recruitment.Application.JobApplications.Notes.Create;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record CreateNote(Guid JobApplicationId, Guid OrganizationId, string Text, Guid AddedBy);
