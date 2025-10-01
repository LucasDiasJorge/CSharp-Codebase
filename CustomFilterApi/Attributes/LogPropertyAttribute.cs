namespace CustomFilterApi.Attributes;

/// <summary>
/// Atributo customizado que marca propriedades de um modelo que devem ser logadas.
/// 
/// Este atributo é aplicado em propriedades de classes que representam DTOs/Models
/// e sinaliza ao filtro que aquela propriedade específica deve ter seu valor capturado
/// e registrado no log.
/// 
/// Exemplo de uso:
/// public class UserDto 
/// {
///     [LogProperty]
///     public string Username { get; set; }
///     
///     public string Password { get; set; } // Esta NÃO será logada
/// }
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class LogPropertyAttribute : Attribute
{
    /// <summary>
    /// Nome customizado para o log. Se não informado, usa o nome da propriedade.
    /// </summary>
    public string? LogName { get; set; }

    /// <summary>
    /// Indica se o valor deve ser mascarado parcialmente no log (útil para dados sensíveis).
    /// </summary>
    public bool MaskValue { get; set; }

    public LogPropertyAttribute()
    {
    }

    public LogPropertyAttribute(string logName)
    {
        LogName = logName;
    }
}
