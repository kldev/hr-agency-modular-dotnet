namespace HrAgencySystem.FileService.Contracts;

/// <summary>
/// The file service could not be reached or refused the call. Distinct from "no such file", which is
/// a null result: this one means the storage itself is the problem, and the caller should say so
/// rather than pretend the document is missing.
/// </summary>
public sealed class FileServiceException(string message, System.Exception? innerException = null)
    : System.Exception(message, innerException)
{
    public const string UnavailableMessage = "Document storage is unavailable.";
    public const string RejectedMessage = "Document storage rejected the file.";
}
