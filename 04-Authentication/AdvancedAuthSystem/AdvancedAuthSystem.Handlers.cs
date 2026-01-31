using Microsoft.AspNetCore.Authorization;
using AdvancedAuthSystem.Authorization.Requirements;
using AdvancedAuthSystem.Models;
using AdvancedAuthSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdvancedAuthSystem.Authorization.Handlers;

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        var dobClaim = context.User.FindFirst("DateOfBirth");
        if (dobClaim == null)
            return Task.CompletedTask;

        if (!DateTime.TryParse(dobClaim.Value, out var dateOfBirth))
            return Task.CompletedTask;

        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
            age--;

        if (age >= requirement.MinimumAge)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public class DepartmentHandler : AuthorizationHandler<DepartmentRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentRequirement requirement)
    {
        var departmentClaim = context.User.FindFirst("Department");
        if (departmentClaim == null)
            return Task.CompletedTask;

        if (departmentClaim.Value.Equals(requirement.Department, StringComparison.OrdinalIgnoreCase))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement, Resource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        Resource resource)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Task.CompletedTask;

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return Task.CompletedTask;

        // Owner can access
        if (resource.OwnerId == userId)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Admin can access all resources
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Manager can access resources in their department
        if (context.User.IsInRole("Manager"))
        {
            var departmentClaim = context.User.FindFirst("Department");
            if (departmentClaim != null && 
                departmentClaim.Value.Equals(resource.Department, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

public class TimeBasedHandler : AuthorizationHandler<TimeBasedRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TimeBasedRequirement requirement)
    {
        var currentTime = DateTime.Now.TimeOfDay;
        var currentDay = DateTime.Now.DayOfWeek;

        // Check allowed days
        if (requirement.AllowedDays != null && !requirement.AllowedDays.Contains(currentDay))
            return Task.CompletedTask;

        // Check time range
        if (currentTime >= requirement.StartTime && currentTime <= requirement.EndTime)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public class ClaimHandler : AuthorizationHandler<ClaimRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClaimRequirement requirement)
    {
        var claim = context.User.FindFirst(requirement.ClaimType);
        if (claim != null && claim.Value.Equals(requirement.ClaimValue, StringComparison.OrdinalIgnoreCase))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return;

        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .ToListAsync();

        var hasPermission = userRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.Name.Equals(requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
            context.Succeed(requirement);
    }
}
