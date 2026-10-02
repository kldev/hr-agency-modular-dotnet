namespace HrAgencySystem.Files.Model;

public sealed class FileInput(Stream stream, string contentType)
{
    public string ContentType { get; } = contentType;

    public Stream Content { get; } = stream;
}
