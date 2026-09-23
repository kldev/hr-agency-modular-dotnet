namespace HrAgencySystem.ReportsService.Contracts;

/// <summary>
/// The reports service could not be reached or failed. The API answers 503: the report is not
/// missing, the thing that computes it is down.
/// </summary>
public sealed class ReportsServiceException(string message, Exception? inner = null)
    : Exception(message, inner)
{
    public const string UnavailableMessage = "The reports service is unavailable.";
}
