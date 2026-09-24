using System.Diagnostics;
using ConfigurationOptionsDemo.Configuration;
using Microsoft.Extensions.Options;

namespace ConfigurationOptionsDemo.Demo;

/// <summary>
/// Um arquivo de configuração próprio dos cenários de recarga, criado em
/// <see cref="AppContext.BaseDirectory"/> e apagado no fim.
///
/// Existe para que os cenários 3 e 4 possam reescrever o arquivo sem tocar no
/// <c>appsettings.json</c> do projeto.
/// </summary>
public sealed class ReloadableConfigFile : IDisposable
{
    public const string FileName = "appsettings.Recarregavel.json";

    public ReloadableConfigFile()
    {
        Path = System.IO.Path.Combine(AppContext.BaseDirectory, FileName);

        SetPort(1025);
    }

    /// <summary>Nome do arquivo, relativo à base — é o que o provedor JSON espera.</summary>
    public string Name => FileName;

    public string Path { get; }

    public void SetPort(int port)
    {
        File.WriteAllText(
            Path,
            $$"""
            {
              "Smtp": {
                "Host": "localhost",
                "Port": {{port}},
                "SenderEmail": "no-reply@exemplo.com",
                "TimeoutSeconds": 30
              }
            }
            """);
    }

    /// <summary>
    /// Espera a recarga chegar. A detecção é por <c>FileSystemWatcher</c>, então é
    /// assíncrona e leva algumas centenas de milissegundos — não dá para gravar o
    /// arquivo e ler o valor novo na linha seguinte.
    /// </summary>
    public async Task<bool> WaitForReloadAsync(IOptionsMonitor<SmtpOptions> monitor, int expectedPort, int timeoutMs = 5_000)
    {
        Stopwatch watch = Stopwatch.StartNew();

        while (watch.ElapsedMilliseconds < timeoutMs)
        {
            if (monitor.CurrentValue.Port == expectedPort)
            {
                return true;
            }

            await Task.Delay(50);
        }

        return false;
    }

    public void Dispose()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }
}
