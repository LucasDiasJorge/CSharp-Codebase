namespace AdvancedAuthSystem.Authorization.Policies;

public static class PolicyNames
{
    // Age-based policies
    public const string MinimumAge18 = "MinimumAge18";
    public const string MinimumAge21 = "MinimumAge21";
    
    // Department policies
    public const string ITDepartment = "ITDepartment";
    public const string SalesDepartment = "SalesDepartment";
    public const string HRDepartment = "HRDepartment";
    
    // Level policies
    public const string SeniorLevel = "SeniorLevel";
    public const string JuniorLevel = "JuniorLevel";
    
    // Time-based policies
    public const string WorkingHours = "WorkingHours";
    public const string WeekdaysOnly = "WeekdaysOnly";
    
    // Resource policies
    public const string ResourceOwner = "ResourceOwner";
    public const string ResourceManager = "ResourceManager";
    
    // Combined policies
    public const string AdminOrManager = "AdminOrManager";
    public const string CanManageUsers = "CanManageUsers";
}
