using CustomFilterApi.Filters;
using CustomFilterApi.Models;
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

    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;
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
        // O filtro já capturou e logou as propriedades marcadas com [LogProperty]
        // antes de chegar aqui

        _logger.LogInformation("Processando criação do usuário no controller");

        // Simulação de processamento
        return Ok(new
        {
            Message = "Usuário criado com sucesso",
            Username = user.Username,
            Email = user.Email,
            Timestamp = DateTime.UtcNow
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
