using System.ComponentModel.DataAnnotations;

namespace ConfigurationOptionsDemo.Configuration;

/// <summary>
/// A seção <c>Smtp</c> do arquivo de configuração, como classe.
///
/// As anotações não são decoração: com <c>ValidateDataAnnotations().ValidateOnStart()</c>
/// elas transformam configuração errada em falha no start, com mensagem, em vez de
/// <c>NullReferenceException</c> na primeira tentativa de enviar e-mail.
/// </summary>
public sealed class SmtpOptions
{
    /// <summary>
    /// Nome da seção. Constante porque o mesmo literal aparece no registro e nos
    /// testes — e erro de digitação em <c>GetSection</c> não falha, só devolve vazio.
    /// </summary>
    public const string SectionName = "Smtp";

    [Required(ErrorMessage = "Smtp:Host e obrigatorio")]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "Smtp:Port precisa estar entre 1 e 65535")]
    public int Port { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Smtp:SenderEmail precisa ser um e-mail valido")]
    public string SenderEmail { get; set; } = string.Empty;

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Vem de user secrets em desenvolvimento e do cofre do provedor em produção —
    /// nunca do <c>appsettings.json</c>, que vai versionado.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    public override string ToString() =>
        $"Host={Host}, Port={Port}, SenderEmail={SenderEmail}, TimeoutSeconds={TimeoutSeconds}, Password={Mask(Password)}";

    /// <summary>
    /// Segredo em log é vazamento. Mascarar aqui, e não em cada ponto de log, é o que
    /// torna difícil esquecer.
    /// </summary>
    private static string Mask(string secret) =>
        string.IsNullOrEmpty(secret) ? "(vazio)" : new string('*', Math.Min(secret.Length, 8));
}
