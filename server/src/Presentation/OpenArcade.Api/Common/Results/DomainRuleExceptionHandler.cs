using Microsoft.AspNetCore.Diagnostics;
using OpenArcade.Domain.Common;

namespace OpenArcade.Api.Common.Results;

/// <summary>Turns a broken business rule from the Domain into a 400 problem response.</summary>
internal sealed class DomainRuleExceptionHandler(IProblemDetailsService problems) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        var status = exception switch
        {
            DomainRuleException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => 0,
        };
        if (status == 0) return false;

        http.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            Exception = exception,
            ProblemDetails = { Status = status, Detail = status == 400 ? exception.Message : null },
        });
    }
}
