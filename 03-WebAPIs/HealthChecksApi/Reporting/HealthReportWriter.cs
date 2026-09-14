using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksApi.Reporting;

/// <summary>
/// Resposta em JSON para as sondas. O writer padrão do ASP.NET Core escreve apenas a
/// palavra do status; para diagnóstico é preciso saber qual check falhou e por quê.
/// </summary>
public static class HealthReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        List<object> checks = new List<object>(report.Entries.Count);
        foreach (KeyValuePair<string, HealthReportEntry> entry in report.Entries)
        {
            checks.Add(new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 1),
                description = entry.Value.Description,
                tags = entry.Value.Tags,

                // A excecao vai para o log, nunca para o corpo: /health costuma estar
                // exposto para a rede interna inteira.
                data = entry.Value.Data
            });
        }

        object payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            checks
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
