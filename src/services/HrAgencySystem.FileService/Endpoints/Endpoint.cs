namespace HrAgencySystem.FileService.Endpoints;

internal static class Endpoint
{
    internal static void MapFileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("").WithTags("Files");

        Maps.MapUpload.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapDownload.Map(group);
        Maps.MapDelete.Map(group);

        app.MapGet(ServiceEndpoints.Health, () => Results.Ok("healthy")).AllowAnonymous();
    }
}
