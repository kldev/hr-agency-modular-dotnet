using HrAgencySystem.Api.Common.Errors;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Common;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder ProducesStandardErrors(
        this RouteHandlerBuilder builder)
    {
        return builder
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}