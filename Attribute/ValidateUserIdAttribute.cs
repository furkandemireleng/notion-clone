using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using notion_clone.Extentions;

namespace notion_clone.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ValidateUserIdAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var routeData = context.HttpContext.Request.RouteValues;
        var userIdFromRoute = routeData["userId"]?.ToString();

        var user = context.HttpContext.User;
        var userIdFromClaims = user.GetUserId();

        if (!string.Equals(userIdFromRoute, userIdFromClaims, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        base.OnActionExecuting(context);
    }
}