namespace HrAgencySystem.Files.Model;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class FileInput(Stream stream, string fileName, string contentType)
{
    public string FileName { get; init; } = fileName;
    public string ContentType { get; init; } = contentType;

    public Stream Content { get; init; } = stream;
}