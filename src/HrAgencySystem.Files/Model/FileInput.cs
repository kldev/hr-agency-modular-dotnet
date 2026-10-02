namespace HrAgencySystem.Files.Model;

public sealed class FileInput(Stream stream, string contentType)
{
    public string ContentType { get; init; } = contentType;

    public Stream Content { get; init; } = stream;
}
