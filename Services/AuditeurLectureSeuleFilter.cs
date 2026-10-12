using Microsoft.AspNetCore.Mvc.Filters;

namespace AkarangaBank.Web.Services;

/// <summary>
/// Empêche un auditeur d'atteindre une action d'écriture, y compris lorsque
/// cette action n'a pas encore de politique d'autorisation dédiée.
/// </summary>
public sealed class AuditeurLectureSeuleFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> ActionsDeFormulaire = new(StringComparer.OrdinalIgnoreCase)
    {
        "Create",
        "Edit",
        "Delete"
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.IsInRole("auditeur"))
        {
            var method = context.HttpContext.Request.Method;
            var action = context.ActionDescriptor.RouteValues.TryGetValue("action", out var actionName)
                ? actionName
                : null;

            if (method is not ("GET" or "HEAD") || (action is not null && ActionsDeFormulaire.Contains(action)))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
                return;
            }
        }

        await next();
    }
}
