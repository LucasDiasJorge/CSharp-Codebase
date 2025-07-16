using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

public class SameUserOrAdminHandler : AuthorizationHandler<SameUserOrAdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameUserOrAdminRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst("userId")?.Value;
        var roleClaim = context.User.FindFirst("role")?.Value;

        if (roleClaim == "Admin")
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.Resource is AuthorizationFilterContext mvcContext)
        {
            var routeId = mvcContext.RouteData.Values["id"]?.ToString();
            if (routeId != null && userIdClaim == routeId)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

public class SameUserOrAdminRequirement : IAuthorizationRequirement { }