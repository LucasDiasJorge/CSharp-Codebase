using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ProblemDetailsApi.Handlers;

/// <summary>
/// Último handler da cadeia: qualquer exceção não prevista vira 500 com um corpo
/// genérico. O que o cliente recebe muda conforme o ambiente — em desenvolvimento, a
/// mensagem real ajuda; em produção, ela é vazamento de informação.
/// </summary>
public sealed class UnhandledExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<UnhandledExceptionHandler> _logger;

    public UnhandledExceptionHandler(
        IProblemDetailsService problemDetailsService,
        IHostEnvironment environment,
        ILogger<UnhandledExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Aqui é incidente de verdade: Error, com a exceção inteira para o log.
        _logger.LogError(exception, "Falha nao tratada em {Path}.", httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        ProblemDetails problemDetails = new ProblemDetails
        {
            Type = "https://example.com/erros/falha-interna",
            Title = "Erro interno",
            Status = StatusCodes.Status500InternalServerError,
            Detail = _environment.IsDevelopment()
                ? exception.Message
                : "A requisicao nao pode ser concluida. Use o traceId ao acionar o suporte.",
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}
