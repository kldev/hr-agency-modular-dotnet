namespace HrAgencySystem.Api.Endpoints.User;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithSummary("Users").WithTags("Users");

        Maps.MapCreate.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapChangeRole.Map(group);
        Maps.MapChangePassword.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapGetSlice.Map(group);

        Maps.MapGetMe.Map(group);
        Maps.MapUpdateMe.Map(group);
        Maps.MapUploadAvatar.Map(group);
        Maps.MapDownloadAvatar.Map(group);
        Maps.MapRemoveAvatar.Map(group);

        Maps.MapGetAvatars.Map(group);
        Maps.MapUploadAvatarFor.Map(group);
        Maps.MapDownloadAvatarFor.Map(group);
        Maps.MapRemoveAvatarFor.Map(group);
    }
}
