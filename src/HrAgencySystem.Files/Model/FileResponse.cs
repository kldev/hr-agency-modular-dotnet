namespace HrAgencySystem.Files.Model;


public class FileResponse
{
    public static readonly Func<Stream, string, FileResponse> SuccessAction = (stream, contentType) =>
        new FileResponse
        {
            OutputStream = stream,
            ContentType = contentType,
            FileNotFound = false,
            FileNotFoundMessage = string.Empty
        };

    public static readonly Func<string, FileResponse> FailureAction = message => new FileResponse
    {
        OutputStream = null,
        FileNotFound = true,
        FileNotFoundMessage = message,
        ContentType = "plain/text"
    };

    public Stream? OutputStream { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public bool FileNotFound { get; set; } = false;
    public string FileNotFoundMessage { get; set; } = string.Empty;
}