using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthSystem.Authorization.Requirements;

public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public int MinimumAge { get; }

    public MinimumAgeRequirement(int minimumAge)
    {
        MinimumAge = minimumAge;
    }
}

public class DepartmentRequirement : IAuthorizationRequirement
{
    public string Department { get; }

    public DepartmentRequirement(string department)
    {
        Department = department;
    }
}

public class ResourceOwnerRequirement : IAuthorizationRequirement
{
}

public class TimeBasedRequirement : IAuthorizationRequirement
{
    public TimeSpan StartTime { get; }
    public TimeSpan EndTime { get; }
    public DayOfWeek[]? AllowedDays { get; }

    public TimeBasedRequirement(TimeSpan startTime, TimeSpan endTime, DayOfWeek[]? allowedDays = null)
    {
        StartTime = startTime;
        EndTime = endTime;
        AllowedDays = allowedDays;
    }
}

public class ClaimRequirement : IAuthorizationRequirement
{
    public string ClaimType { get; }
    public string ClaimValue { get; }

    public ClaimRequirement(string claimType, string claimValue)
    {
        ClaimType = claimType;
        ClaimValue = claimValue;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
