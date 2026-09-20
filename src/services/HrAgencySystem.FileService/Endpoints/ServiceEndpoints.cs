namespace HrAgencySystem.FileService.Endpoints;

internal static class ServiceEndpoints
{
    private const string Base = "/files";

    public const string Health = "/healthz";

    public const string Upload = Base;
    public const string Get = $"{Base}/{{fileId:guid}}";
    public const string Content = $"{Base}/{{fileId:guid}}/content";
    public const string Delete = $"{Base}/{{fileId:guid}}";
}
