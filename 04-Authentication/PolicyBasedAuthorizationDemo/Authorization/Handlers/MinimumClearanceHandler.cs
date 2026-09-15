using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuthorizationDemo.Authorization.Requirements;

namespace PolicyBasedAuthorizationDemo.Authorization.Handlers;

/// <summary>
/// Avalia o nível de acesso declarado na claim. Repare no que o handler NÃO faz: ele não
/// chama <c>context.Fail()</c> quando a claim é insuficiente. Simplesmente não chama
/// <c>Succeed</c>, e a exigência fica pendente — o que deixa a porta aberta para outro
/// handler satisfazê-la. <c>Fail()</c> é definitivo e venceria qualquer sucesso.
/// </summary>
public sealed class MinimumClearanceHandler : AuthorizationHandler<MinimumClearanceRequirement>
{
    public const string ClearanceClaim = "clearance";

    private readonly ILogger<MinimumClearanceHandler> _logger;

    public MinimumClearanceHandler(ILogger<MinimumClearanceHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumClearanceRequirement requirement)
    {
        Claim? claim = context.User.FindFirst(ClearanceClaim);

        if (claim is null || !int.TryParse(claim.Value, out int clearance))
        {
            _logger.LogInformation("Usuario sem claim {Claim} valida; exigencia continua pendente.", ClearanceClaim);

            return Task.CompletedTask;
        }

        if (clearance >= requirement.Level)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogInformation(
                "Nivel {Atual} abaixo do exigido {Exigido}; exigencia continua pendente.",
                clearance,
                requirement.Level);
        }

        return Task.CompletedTask;
    }
}
