using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Common.Response;

internal static class DomainObjectNotFound
{
    internal static ProblemDetails NotFound(string domain, Guid key )
    {
        return new ProblemDetails()
        {
            Title = "Not found",
            Detail = $"{domain} not found by {key}", Status = (int)HttpStatusCode.NotFound
        };
    }
    
    internal static ProblemDetails NotFound(string domain, string key )
    {
        return new ProblemDetails()
        {
            Title = "Not found",
            Detail = $"{domain} not found by {key}", Status = (int)HttpStatusCode.NotFound
        };
    }
    
}

