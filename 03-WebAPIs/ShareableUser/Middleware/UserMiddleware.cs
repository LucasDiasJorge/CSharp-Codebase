public class UsuarioMonitorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UsuarioMonitorMiddleware> _logger;

    public UsuarioMonitorMiddleware(
        RequestDelegate next,
        ILogger<UsuarioMonitorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUsuarioSingletonService usuarioService)
    {
        // Gera um ID único para esta requisição
        var requestId = Guid.NewGuid().ToString()[..8];
        
        // Registra acesso ao usuário singleton
        usuarioService.AcessarUsuario(requestId);
        
        // Log inicial
        _logger.LogInformation(
            $"Requisição {requestId} acessando usuário singleton (ID: {usuarioService.InstanceId}, Acessos: {usuarioService.ContadorAcessos})");
        
        // Simula processamento para demonstrar concorrência
        await Task.Delay(new Random().Next(50, 200));
        
        // Continua o pipeline
        await _next(context);
        
        // Log final
        _logger.LogInformation(
            $"Requisição {requestId} concluída. Total acessos: {usuarioService.ContadorAcessos}");
    }
}