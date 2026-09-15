using Microsoft.AspNetCore.Authorization;

namespace PolicyBasedAuthorizationDemo.Authorization.Requirements;

/// <summary>
/// Exigência avaliada contra um recurso específico. Não dá para expressá-la em atributo:
/// "pode editar este documento" depende de qual documento é, e isso só se sabe depois de
/// carregá-lo.
/// </summary>
public sealed class DocumentEditRequirement : IAuthorizationRequirement
{
    public static readonly DocumentEditRequirement Instance = new DocumentEditRequirement();
}
