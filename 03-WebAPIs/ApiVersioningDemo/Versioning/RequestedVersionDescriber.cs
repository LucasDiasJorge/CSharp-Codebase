using Asp.Versioning;

namespace ApiVersioningDemo.Versioning;

/// <summary>
/// Lê a versão resolvida para a requisição atual. Existe para mostrar onde essa
/// informação mora: <see cref="IApiVersioningFeature"/>, preenchida pelo middleware
/// depois que os readers configurados encontraram (ou não) um valor.
/// </summary>
public static class RequestedVersionDescriber
{
    public static string Describe(HttpContext context)
    {
        IApiVersioningFeature? feature = context.Features.Get<IApiVersioningFeature>();

        if (feature is null)
        {
            return "indefinida";
        }

        ApiVersion? resolved = feature.RequestedApiVersion;
        string? raw = feature.RawRequestedApiVersion;

        if (raw is null)
        {
            // Nenhum reader encontrou valor: a versão veio de DefaultApiVersion.
            return $"{resolved?.ToString() ?? "indefinida"} (padrao, nada foi informado na requisicao)";
        }

        return $"{resolved?.ToString() ?? "indefinida"} (valor bruto recebido: {raw})";
    }
}
