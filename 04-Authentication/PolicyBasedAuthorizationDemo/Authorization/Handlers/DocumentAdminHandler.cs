using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuthorizationDemo.Authorization.Requirements;
using PolicyBasedAuthorizationDemo.Documents;

namespace PolicyBasedAuthorizationDemo.Authorization.Handlers;

/// <summary>
/// Segundo caminho para a MESMA exigência: ser admin. Vários handlers registrados para
/// um requirement funcionam em OU — basta um chamar <c>Succeed</c>. Para exigir as duas
/// condições ao mesmo tempo, o caminho é outro: dois requirements na mesma policy.
/// </summary>
public sealed class DocumentAdminHandler : AuthorizationHandler<DocumentEditRequirement, Document>
{
    private readonly ILogger<DocumentAdminHandler> _logger;

    public DocumentAdminHandler(ILogger<DocumentAdminHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DocumentEditRequirement requirement,
        Document resource)
    {
        if (context.User.IsInRole("admin"))
        {
            _logger.LogInformation("Acesso ao documento {DocumentoId} concedido por perfil admin.", resource.Id);
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
