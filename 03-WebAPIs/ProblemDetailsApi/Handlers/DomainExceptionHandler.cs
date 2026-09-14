using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProblemDetailsApi.Domain;

namespace ProblemDetailsApi.Handlers;

/// <summary>
/// Primeiro handler da cadeia: traduz falhas previstas do domínio para o status HTTP
/// correspondente. Devolver <c>false</c> em qualquer outro caso passa a bola para o
/// próximo handler registrado.
/// </summary>
public sealed class DomainExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<DomainExceptionHandler> _logger;

    public DomainExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<DomainExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException)
        {
            // Não é falha prevista: deixa para o handler seguinte.
            return false;
        }

        // Falha de negócio é resultado esperado, não incidente: Warning, não Error.
        _logger.LogWarning(
            "Falha de dominio {ErrorType} em {Path}: {Mensagem}",
            domainException.ErrorType,
            httpContext.Request.Path,
            domainException.Message);

        httpContext.Response.StatusCode = domainException.StatusCode;

        ProblemDetails problemDetails = new ProblemDetails
        {
            Type = domainException.ErrorType,
            Title = domainException.Title,
            Status = domainException.StatusCode,
            Detail = domainException.Message,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        foreach (KeyValuePair<string, object?> extension in domainException.Extensions)
        {
            problemDetails.Extensions[extension.Key] = extension.Value;
        }

        // Escrever pelo IProblemDetailsService, e não serializar à mão, garante que as
        // customizações de CustomizeProblemDetails também valham aqui.
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = domainException
        });
    }
}
