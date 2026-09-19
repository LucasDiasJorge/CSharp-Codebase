using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ProxyRemoteServiceDemo.Contracts;
using ProxyRemoteServiceDemo.Proxies;
using ProxyRemoteServiceDemo.Remote;

namespace ProxyRemoteServiceDemo.Demo;

/// <summary>
/// Cinco cenários. O fio condutor é que <see cref="RunClientAsync"/> — o código do
/// cliente — é exatamente o mesmo em todos eles, recebendo sempre um
/// <see cref="IReportService"/> e sem jamais saber o que há por trás.
/// </summary>
public sealed class ProxyDemoRunner
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ProxyDemoRunner> _logger;

    public ProxyDemoRunner(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ProxyDemoRunner>();
    }

    public async Task RunAllAsync()
    {
        await RunDirectAsync();
        await RunLazyNeverUsedAsync();
        await RunLazyUsedAsync();
        await RunAuthorizationAsync();
        await RunComposedAsync();
    }

    /// <summary>
    /// O CLIENTE. Não muda em nenhum cenário: recebe a interface e chama. É a medida
    /// de sucesso do padrão.
    /// </summary>
    private async Task RunClientAsync(IReportService service, string label)
    {
        int available = await service.CountAvailableAsync(CancellationToken.None);
        string report = await service.GenerateAsync("vendas-mensal", CancellationToken.None);

        _logger.LogInformation("  [{Cenario}] {Disponiveis} relatorios disponiveis; gerado: {Relatorio}", label, available, report);
    }

    private async Task RunDirectAsync()
    {
        Section("1. Sem proxy: o custo da conexao e pago na construcao");

        RemoteReportService.ResetCounter();
        Stopwatch watch = Stopwatch.StartNew();

        IReportService service = new RemoteReportService(_loggerFactory.CreateLogger<RemoteReportService>());
        _logger.LogInformation("Servico construido em {Tempo}ms, antes de qualquer uso.", watch.ElapsedMilliseconds);

        await RunClientAsync(service, "direto");
    }

    private async Task RunLazyNeverUsedAsync()
    {
        Section("2. Proxy virtual, servico nunca usado: conexao nao e aberta");

        RemoteReportService.ResetCounter();
        Stopwatch watch = Stopwatch.StartNew();

        LazyConnectionProxy proxy = new LazyConnectionProxy(
            () => new RemoteReportService(_loggerFactory.CreateLogger<RemoteReportService>()),
            _loggerFactory.CreateLogger<LazyConnectionProxy>());

        _logger.LogInformation(
            "Proxy construido em {Tempo}ms. Conectado? {Conectado}. Instancias reais criadas: {Instancias}.",
            watch.ElapsedMilliseconds,
            proxy.IsConnected,
            RemoteReportService.InstancesCreated);

        await Task.CompletedTask;
    }

    private async Task RunLazyUsedAsync()
    {
        Section("3. Proxy virtual, servico usado: conexao aberta na primeira chamada");

        RemoteReportService.ResetCounter();

        LazyConnectionProxy proxy = new LazyConnectionProxy(
            () => new RemoteReportService(_loggerFactory.CreateLogger<RemoteReportService>()),
            _loggerFactory.CreateLogger<LazyConnectionProxy>());

        await RunClientAsync(proxy, "lazy");

        // Segunda rodada: a conexao ja existe e nao e criada de novo.
        await RunClientAsync(proxy, "lazy-2a-vez");

        _logger.LogInformation(
            "Apos duas rodadas: {Instancias} instancia(s) real(is) criada(s) — o custo foi pago uma vez so.",
            RemoteReportService.InstancesCreated);
    }

    private async Task RunAuthorizationAsync()
    {
        Section("4. Proxy de protecao: barra antes de chegar ao servico real");

        RemoteReportService.ResetCounter();

        CallerContext semPermissao = new CallerContext("visitante", []);
        CallerContext comPermissao = new CallerContext("analista", [AuthorizationProxy.GeneratePermission, AuthorizationProxy.ReadPermission]);

        IReportService lazy = new LazyConnectionProxy(
            () => new RemoteReportService(_loggerFactory.CreateLogger<RemoteReportService>()),
            _loggerFactory.CreateLogger<LazyConnectionProxy>());

        IReportService blocked = new AuthorizationProxy(lazy, semPermissao, _loggerFactory.CreateLogger<AuthorizationProxy>());

        try
        {
            await RunClientAsync(blocked, "sem-permissao");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Bloqueado: {Erro}", ex.Message);
        }

        _logger.LogInformation(
            "Instancias reais criadas apos a tentativa negada: {Instancias} — a conexao nem chegou a ser aberta.",
            RemoteReportService.InstancesCreated);

        IReportService allowed = new AuthorizationProxy(lazy, comPermissao, _loggerFactory.CreateLogger<AuthorizationProxy>());
        await RunClientAsync(allowed, "com-permissao");
    }

    private async Task RunComposedAsync()
    {
        Section("5. Tres proxies encadeados, cliente inalterado");

        RemoteReportService.ResetCounter();

        CallerContext caller = new CallerContext("analista", [AuthorizationProxy.GeneratePermission, AuthorizationProxy.ReadPermission]);

        // A ordem da cadeia importa: autorizacao por fora barra antes de medir e antes
        // de conectar; telemetria por dentro mede o tempo real da chamada remota.
        IReportService lazy = new LazyConnectionProxy(
            () => new RemoteReportService(_loggerFactory.CreateLogger<RemoteReportService>()),
            _loggerFactory.CreateLogger<LazyConnectionProxy>());

        TelemetryProxy telemetry = new TelemetryProxy(lazy, _loggerFactory.CreateLogger<TelemetryProxy>());
        IReportService chain = new AuthorizationProxy(telemetry, caller, _loggerFactory.CreateLogger<AuthorizationProxy>());

        await RunClientAsync(chain, "cadeia");

        _logger.LogInformation("Telemetria coletou {Quantidade} chamada(s):", telemetry.Records.Count);
        foreach (CallRecord record in telemetry.Records)
        {
            _logger.LogInformation("  {Operacao}: {Tempo}ms, sucesso={Sucesso}", record.Operation, record.ElapsedMs, record.Succeeded);
        }

        _logger.LogInformation("O metodo do cliente foi o mesmo nos cinco cenarios — nunca soube que havia proxy algum.");
    }

    private void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
