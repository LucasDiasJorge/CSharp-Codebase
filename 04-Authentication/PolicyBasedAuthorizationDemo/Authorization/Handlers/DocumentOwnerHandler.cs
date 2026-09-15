using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuthorizationDemo.Authorization.Requirements;
using PolicyBasedAuthorizationDemo.Documents;

namespace PolicyBasedAuthorizationDemo.Authorization.Handlers;

/// <summary>
/// Primeiro caminho para satisfazer a edição: ser o dono do documento. O tipo genérico
/// de dois parâmetros recebe o recurso concreto, coisa que um atributo nunca teria.
/// </summary>
public sealed class DocumentOwnerHandler : AuthorizationHandler<DocumentEditRequirement, Document>
{
    private readonly ILogger<DocumentOwnerHandler> _logger;

    public DocumentOwnerHandler(ILogger<DocumentOwnerHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DocumentEditRequirement requirement,
        Document resource)
    {
        string? userId = context.User.FindFirst("sub")?.Value;

        if (userId is not null && resource.OwnerId == userId)
        {
            _logger.LogInformation("Usuario {UsuarioId} e dono do documento {DocumentoId}.", userId, resource.Id);
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
