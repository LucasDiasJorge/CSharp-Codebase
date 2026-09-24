using ConfigurationOptionsDemo.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConfigurationOptionsDemo.Demo;

/// <summary>
/// As camadas de configuração e de onde cada valor final veio.
/// </summary>
public sealed class LayeringScenarios
{
    /// <summary>
    /// Prefixo no provedor de variáveis de ambiente. Sem prefixo, TODAS as variáveis
    /// da máquina entram na configuração — inclusive as que ninguém quer ver em log.
    /// </summary>
    private const string EnvironmentPrefix = "CFGDEMO_";

    private readonly ILogger<LayeringScenarios> _logger;

    public LayeringScenarios(ILogger<LayeringScenarios> logger)
    {
        _logger = logger;
    }

    public void RunPrecedence()
    {
        Section("1. Camadas: a ultima que fala, vence");

        // Uma variavel de ambiente definida aqui para o exemplo ser deterministico.
        // O separador e DOIS sublinhados: ':' nao e valido em nome de variavel em
        // varios sistemas.
        Environment.SetEnvironmentVariable($"{EnvironmentPrefix}Smtp__Port", "2525");

        _logger.LogInformation("  o que cada camada diz sobre Smtp:Host e Smtp:Port:");

        LogLayer("appsettings.json", BuildSingleLayer(builder => builder.AddJsonFile("appsettings.json")));
        LogLayer("appsettings.Development.json", BuildSingleLayer(builder => builder.AddJsonFile("appsettings.Development.json")));
        LogLayer($"variaveis {EnvironmentPrefix}*", BuildSingleLayer(builder => builder.AddEnvironmentVariables(EnvironmentPrefix)));

        IConfigurationRoot configuration = BuildFullStack();

        _logger.LogInformation(
            "  resultado combinado: Host={Host}, Port={Port}",
            configuration["Smtp:Host"],
            configuration["Smtp:Port"]);

        _logger.LogInformation("  quem venceu cada chave (GetDebugView, filtrado na secao Smtp):");

        foreach (string line in configuration.GetDebugView().Split(Environment.NewLine))
        {
            if (line.Contains("Host=", StringComparison.Ordinal)
                || line.Contains("Port=", StringComparison.Ordinal)
                || line.Contains("SenderEmail=", StringComparison.Ordinal))
            {
                _logger.LogInformation("    {Linha}", line.Trim());
            }
        }

        _logger.LogInformation(
            "A ordem de registro e a ordem de precedencia: cada provedor sobrescreve os anteriores. Host veio do arquivo de Development, Port veio da variavel de ambiente.");

        Environment.SetEnvironmentVariable($"{EnvironmentPrefix}Smtp__Port", null);
    }

    public void RunUserSecrets()
    {
        Section("5. User secrets: fora do repositorio, dentro da configuracao");

        string secretsPath = GetUserSecretsPath();
        bool exists = File.Exists(secretsPath);

        _logger.LogInformation("  UserSecretsId: csharp-codebase-configuration-options-demo");
        _logger.LogInformation("  arquivo: {Caminho}", secretsPath);
        _logger.LogInformation("  existe nesta maquina: {Existe}", exists);

        IConfigurationRoot configuration = BuildFullStack();
        string password = configuration["Smtp:Password"] ?? string.Empty;

        if (string.IsNullOrEmpty(password))
        {
            _logger.LogWarning(
                "  Smtp:Password esta vazio — o appsettings.json nao traz segredo, e nenhum secret foi definido.");
            _logger.LogInformation(
                "  para ver a camada em acao: dotnet user-secrets set \"Smtp:Password\" \"s3nh4-local\" --project 11-Utilities/ConfigurationOptionsDemo");
        }
        else
        {
            _logger.LogInformation(
                "  Smtp:Password veio dos user secrets, com {Tamanho} caracteres (nao aparece em nenhum arquivo do repositorio).",
                password.Length);
        }

        _logger.LogInformation(
            "O arquivo fica no perfil do usuario, FORA da pasta do projeto: nao vai em commit, nao vai em imagem de container, nao vaza em ZIP do codigo.");

        _logger.LogWarning(
            "User secrets e so para desenvolvimento — nao e cofre, nao e criptografado, e o JSON fica em texto puro no disco. Em producao, use o cofre do provedor.");
    }

    /// <summary>
    /// A pilha completa, na ordem que importa: arquivo base, arquivo do ambiente,
    /// segredos locais e variáveis de ambiente.
    /// </summary>
    public static IConfigurationRoot BuildFullStack() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables(EnvironmentPrefix)
            .Build();

    private static IConfigurationRoot BuildSingleLayer(Action<IConfigurationBuilder> configure)
    {
        ConfigurationBuilder builder = new ConfigurationBuilder();

        builder.SetBasePath(AppContext.BaseDirectory);
        configure(builder);

        return builder.Build();
    }

    private void LogLayer(string name, IConfigurationRoot configuration)
    {
        _logger.LogInformation(
            "    {Camada,-30}: Host={Host,-26} Port={Port}",
            name,
            configuration["Smtp:Host"] ?? "(ausente)",
            configuration["Smtp:Port"] ?? "(ausente)");
    }

    /// <summary>
    /// Caminho padrão do arquivo de user secrets. Fica no perfil do usuário,
    /// identificado pelo <c>UserSecretsId</c> do <c>.csproj</c>.
    /// </summary>
    private static string GetUserSecretsPath()
    {
        string root = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".microsoft", "usersecrets");

        return Path.Combine(root, "csharp-codebase-configuration-options-demo", "secrets.json");
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
