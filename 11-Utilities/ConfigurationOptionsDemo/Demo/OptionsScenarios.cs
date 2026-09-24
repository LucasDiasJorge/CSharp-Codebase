using ConfigurationOptionsDemo.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConfigurationOptionsDemo.Demo;

/// <summary>
/// O Options Pattern propriamente dito: validação no start e as três interfaces que
/// parecem intercambiáveis e não são.
/// </summary>
public sealed class OptionsScenarios
{
    private readonly ILogger<OptionsScenarios> _logger;

    public OptionsScenarios(ILogger<OptionsScenarios> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Configuração inválida com e sem <c>ValidateOnStart</c>. O mesmo defeito, dois
    /// momentos muito diferentes de descoberta.
    /// </summary>
    public async Task RunValidationAsync()
    {
        Section("2. Validacao: falhar no start ou falhar em producao");

        _logger.LogInformation("  appsettings.Invalido.json: Host vazio, Port 70000, SenderEmail sem formato de e-mail.");

        // Com ValidateOnStart: o host nao sobe.
        using IHost strictHost = BuildHost("appsettings.Invalido.json", validateOnStart: true);

        try
        {
            await strictHost.StartAsync();

            _logger.LogError("  o host subiu — nao era para ter subido.");
        }
        catch (OptionsValidationException exception)
        {
            _logger.LogWarning("  com ValidateOnStart: o host NAO sobe. {Tipo}:", exception.GetType().Name);

            foreach (string failure in exception.Failures)
            {
                _logger.LogWarning("    - {Falha}", failure);
            }
        }

        // Sem ValidateOnStart: o host sobe, e o erro espera o primeiro uso.
        using IHost lazyHost = BuildHost("appsettings.Invalido.json", validateOnStart: false);

        await lazyHost.StartAsync();

        _logger.LogWarning("  sem ValidateOnStart: o host SOBE normalmente, e a aplicacao parece saudavel.");

        try
        {
            SmtpOptions options = lazyHost.Services.GetRequiredService<IOptions<SmtpOptions>>().Value;

            _logger.LogError("  leu as opcoes sem erro: {Opcoes}", options);
        }
        catch (OptionsValidationException)
        {
            _logger.LogWarning(
                "    a mesma excecao acontece — mas so no primeiro acesso as opcoes, ou seja, no primeiro e-mail que alguem tentar enviar.");
        }

        await lazyHost.StopAsync();

        _logger.LogInformation(
            "Mesma configuracao errada: com ValidateOnStart o deploy falha na hora; sem ele, o deploy passa e a falha chega com o trafego.");
    }

    /// <summary>
    /// <c>IOptions</c>, <c>IOptionsSnapshot</c> e <c>IOptionsMonitor</c> com o arquivo
    /// mudando embaixo. A diferença entre eles só aparece aqui.
    /// </summary>
    public async Task RunThreeInterfacesAsync(ReloadableConfigFile file)
    {
        Section("3. IOptions x IOptionsSnapshot x IOptionsMonitor");

        using IHost host = BuildHost(file.Path, validateOnStart: true, reloadOnChange: true);

        await host.StartAsync();

        IOptions<SmtpOptions> singleton = host.Services.GetRequiredService<IOptions<SmtpOptions>>();
        IOptionsMonitor<SmtpOptions> monitor = host.Services.GetRequiredService<IOptionsMonitor<SmtpOptions>>();

        _logger.LogInformation("  valor inicial de Smtp:Port = {Porta}", singleton.Value.Port);

        file.SetPort(9999);

        bool reloaded = await file.WaitForReloadAsync(monitor, expectedPort: 9999);

        _logger.LogInformation("  arquivo alterado para Port=9999 (recarga detectada: {Recarregou})", reloaded);

        using (IServiceScope scope = host.Services.CreateScope())
        {
            IOptionsSnapshot<SmtpOptions> snapshot = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SmtpOptions>>();

            _logger.LogInformation("    IOptions<T>.Value          = {Porta}  (singleton: preso ao primeiro calculo)", singleton.Value.Port);
            _logger.LogInformation("    IOptionsSnapshot<T>.Value  = {Porta}  (scoped: recalculado neste escopo)", snapshot.Value.Port);
            _logger.LogInformation("    IOptionsMonitor<T>.Current = {Porta}  (singleton, sempre atual)", monitor.CurrentValue.Port);
        }

        await host.StopAsync();

        _logger.LogInformation(
            "IOptions e o padrao e basta para o que nao muda. IOptionsSnapshot vale por requisicao. IOptionsMonitor e o unico que serve dentro de um singleton de vida longa.");
    }

    /// <summary>
    /// O callback de mudança e o que não se pode assumir sobre ele: o número de
    /// disparos por gravação não é garantido.
    /// </summary>
    public async Task RunReloadCallbackAsync(ReloadableConfigFile file)
    {
        Section("4. Recarga: o callback e quantas vezes ele dispara");

        using IHost host = BuildHost(file.Path, validateOnStart: true, reloadOnChange: true);

        await host.StartAsync();

        IOptionsMonitor<SmtpOptions> monitor = host.Services.GetRequiredService<IOptionsMonitor<SmtpOptions>>();

        int callbacks = 0;

        using IDisposable? registration = monitor.OnChange(options =>
        {
            int current = Interlocked.Increment(ref callbacks);

            _logger.LogInformation("    callback #{Numero}: Port agora e {Porta}", current, options.Port);
        });

        file.SetPort(7777);
        await file.WaitForReloadAsync(monitor, expectedPort: 7777);

        // Um tempo a mais porque o segundo disparo, quando acontece, chega depois.
        await Task.Delay(700);

        _logger.LogInformation(
            "  uma gravacao no arquivo -> {Quantidade} chamada(s) de callback",
            Volatile.Read(ref callbacks));

        _logger.LogWarning(
            Volatile.Read(ref callbacks) > 1
                ? "  disparou mais de uma vez para UMA gravacao: o FileSystemWatcher ve varios eventos por escrita. O callback precisa ser idempotente."
                : "  aqui deu uma so — mas nao ha garantia disso. Dependendo do editor e do sistema de arquivos, a mesma gravacao gera varios eventos. O callback precisa ser idempotente de qualquer forma.");

        await host.StopAsync();

        _logger.LogInformation(
            "Recarga vale para o que pode mudar a quente (nivel de log, feature flag, timeout). Connection string e porta de escuta nao se recarregam sozinhas — quem ja abriu a conexao nao sabe que a configuracao mudou.");
    }

    /// <summary>
    /// Host mínimo com uma única fonte de configuração, para que cada cenário controle
    /// exatamente o que está sendo lido.
    /// </summary>
    private static IHost BuildHost(string jsonFile, bool validateOnStart, bool reloadOnChange = false)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = AppContext.BaseDirectory,
            DisableDefaults = true,
        });

        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(jsonFile, optional: false, reloadOnChange: reloadOnChange);

        OptionsBuilder<SmtpOptions> options = builder.Services
            .AddOptions<SmtpOptions>()
            .Bind(builder.Configuration.GetSection(SmtpOptions.SectionName))
            .ValidateDataAnnotations();

        if (validateOnStart)
        {
            options.ValidateOnStart();
        }

        return builder.Build();
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
