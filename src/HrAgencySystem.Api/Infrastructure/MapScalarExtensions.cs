using Scalar.AspNetCore;

namespace HrAgencySystem.Api.Infrastructure;

public static class MapScalarExtensions
{
    public static void MapAppScalar(this WebApplication app)
    {
        app.MapScalarApiReference("/docs", options => { options.Title = "HR Agency Platform API"; });
    }
}