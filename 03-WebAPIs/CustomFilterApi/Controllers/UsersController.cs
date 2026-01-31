using CustomFilterApi.Filters;
using CustomFilterApi.Models;
using CustomFilterApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomFilterApi.Controllers;

/// <summary>
/// Controller demonstrando o uso do filtro LogPropertyFilter.
/// 
/// O filtro pode ser aplicado de 3 formas:
/// 1. Global (no Program.cs) - afeta todas as actions
/// 2. No Controller (como aqui) - afeta todas as actions do controller
/// 3. Na Action específica - afeta apenas aquela action
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(LogPropertyFilter))] // Aplica o filtro em todas as actions deste controller
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly ISelectedServiceAccessor _accessor;

    public UsersController(ILogger<UsersController> logger, ISelectedServiceAccessor accessor)
    {
        _logger = logger;
        _accessor = accessor;
    }

    /// <summary>
    /// Cria um novo usuário.
    /// O filtro LogPropertyFilter irá interceptar e logar as propriedades marcadas.
    /// </summary>
    /// <param name="user">Dados do usuário</param>
    /// <returns>Confirmação da criação</returns>
    [HttpPost]
    public IActionResult CreateUser([FromBody] UserDto user)
    {
        var result = _accessor.Selected?.Execute(user) ?? "No service selected";

        _logger.LogInformation("Processando criação do usuário no controller");

        // Simulação de processamento
        return Ok(new
        {
            Message = "Usuário criado com sucesso",
            Username = user.Username,
            Email = user.Email,
            Timestamp = DateTime.UtcNow,
            ServiceResult = result
        });
    }

    /// <summary>
    /// Atualiza dados de um usuário existente.
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="user">Novos dados do usuário</param>
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, [FromBody] UserDto user)
    {
        _logger.LogInformation("Atualizando usuário {UserId}", id);

        return Ok(new
        {
            Message = $"Usuário {id} atualizado com sucesso",
            UpdatedFields = new { user.Username, user.Email, user.Age }
        });
    }

    /// <summary>
    /// Endpoint sem corpo na requisição.
    /// O filtro não encontrará propriedades para logar.
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        return Ok(new
        {
            Id = id,
            Username = "john_doe",
            Email = "john@example.com",
            Age = 30
        });
    }
}
