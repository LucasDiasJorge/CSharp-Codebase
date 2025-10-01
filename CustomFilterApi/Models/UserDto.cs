using CustomFilterApi.Attributes;

namespace CustomFilterApi.Models;

/// <summary>
/// Modelo de exemplo representando um usuário.
/// Demonstra o uso do atributo [LogProperty] em diferentes propriedades.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Username será logado automaticamente pelo filtro
    /// </summary>
    [LogProperty]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email será logado com nome customizado "E-mail do usuário"
    /// </summary>
    [LogProperty(logName: "E-mail do usuário")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password será logado, mas com o valor mascarado para segurança
    /// </summary>
    [LogProperty(MaskValue = true)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Esta propriedade NÃO será logada pois não tem o atributo [LogProperty]
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Age será logado pelo filtro
    /// </summary>
    [LogProperty]
    public int Age { get; set; }
}
