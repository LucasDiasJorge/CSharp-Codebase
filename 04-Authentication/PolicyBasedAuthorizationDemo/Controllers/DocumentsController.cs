using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolicyBasedAuthorizationDemo.Authorization;
using PolicyBasedAuthorizationDemo.Authorization.Requirements;
using PolicyBasedAuthorizationDemo.Documents;

namespace PolicyBasedAuthorizationDemo.Controllers;

[ApiController]
[Route("documents")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly DocumentStore _documents;
    private readonly IAuthorizationService _authorizationService;

    public DocumentsController(DocumentStore documents, IAuthorizationService authorizationService)
    {
        _documents = documents;
        _authorizationService = authorizationService;
    }

    /// <summary>Qualquer usuário autenticado enxerga a lista.</summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<Document>> GetAll()
    {
        return Ok(_documents.GetAll());
    }

    /// <summary>
    /// Autorização por perfil. Serve de contraste com o endpoint seguinte: "admin" é um
    /// rótulo, não uma regra, e mudar quem pode ler exige recompilar.
    /// </summary>
    [HttpGet("by-role")]
    [Authorize(Roles = "admin")]
    public ActionResult ByRole()
    {
        return Ok(new { message = "Liberado por [Authorize(Roles = \"admin\")]." });
    }

    /// <summary>
    /// Mesma intenção do anterior, expressa como policy. A regra fica em um lugar só e
    /// pode mudar sem tocar no controller.
    /// </summary>
    [HttpGet("by-policy")]
    [Authorize(Policy = Policies.ConfidentialAccess)]
    public ActionResult ByPolicy()
    {
        return Ok(new { message = "Liberado pela policy ConfidentialAccess (clearance >= 3)." });
    }

    /// <summary>Policy de claim exata.</summary>
    [HttpGet("engineering")]
    [Authorize(Policy = Policies.EngineeringOnly)]
    public ActionResult Engineering()
    {
        return Ok(new { message = "Liberado pela policy EngineeringOnly." });
    }

    /// <summary>Policy com duas exigências: as duas precisam ser satisfeitas (AND).</summary>
    [HttpGet("senior-engineering")]
    [Authorize(Policy = Policies.SeniorEngineering)]
    public ActionResult SeniorEngineering()
    {
        return Ok(new { message = "Liberado pela policy SeniorEngineering (departamento E clearance)." });
    }

    /// <summary>Regra inline com RequireAssertion.</summary>
    [HttpGet("weekday")]
    [Authorize(Policy = Policies.WeekdayOnly)]
    public ActionResult Weekday()
    {
        return Ok(new { message = "Liberado pela policy WeekdayOnly.", today = DateTime.UtcNow.DayOfWeek.ToString() });
    }

    /// <summary>
    /// Autorização por recurso. O atributo não resolve este caso: a decisão depende do
    /// documento concreto, que só existe depois da consulta. Daí a chamada explícita a
    /// <see cref="IAuthorizationService"/> no meio da ação.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Edit(int id)
    {
        Document? document = _documents.FindById(id);
        if (document is null)
        {
            return NotFound();
        }

        AuthorizationResult result = await _authorizationService.AuthorizeAsync(
            User,
            document,
            DocumentEditRequirement.Instance);

        if (!result.Succeeded)
        {
            // 403 e não 404: o usuário está autenticado, apenas não pode editar este
            // documento. Devolver 404 esconderia a existência do recurso — decisão
            // legítima em alguns domínios, mas que precisa ser deliberada.
            return Forbid();
        }

        return Ok(new { message = "Edicao autorizada.", document = document.Title });
    }

    /// <summary>Mostra as claims que chegaram no token, para conferir os testes.</summary>
    [HttpGet("/whoami")]
    public ActionResult WhoAmI()
    {
        Dictionary<string, string> claims = new Dictionary<string, string>();
        foreach (System.Security.Claims.Claim claim in User.Claims)
        {
            claims[claim.Type] = claim.Value;
        }

        return Ok(new { name = User.Identity?.Name, claims });
    }
}
