using Microsoft.AspNetCore.Authorization;

namespace PolicyBasedAuthorizationDemo.Authorization.Requirements;

/// <summary>
/// Requirement é apenas o dado da exigência — não contém lógica. Quem decide é o
/// handler, e essa separação é o que permite ter várias formas de satisfazer a mesma
/// exigência sem tocar em quem a declarou.
/// </summary>
public sealed class MinimumClearanceRequirement : IAuthorizationRequirement
{
    public MinimumClearanceRequirement(int level)
    {
        Level = level;
    }

    public int Level { get; }
}
