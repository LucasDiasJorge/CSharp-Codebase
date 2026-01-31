using CustomFilterApi.Attributes;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using CustomFilterApi.Services;

namespace CustomFilterApi.Filters;

/// <summary>
/// Action Filter customizado que intercepta requisições e captura propriedades 
/// marcadas com o atributo [LogProperty] dos modelos recebidos no payload.
/// 
/// Este filtro demonstra o funcionamento de filtros no ASP.NET Core e como
/// usar reflexão para inspecionar objetos em tempo de execução.
/// 
/// COMO FUNCIONA:
/// 1. OnActionExecuting é executado ANTES do método do controller
/// 2. Inspeciona todos os argumentos passados para a action
/// 3. Busca propriedades marcadas com [LogProperty]
/// 4. Extrai e loga os valores dessas propriedades
/// </summary>
public class LogPropertyFilter : IActionFilter
{
    private readonly ILogger<LogPropertyFilter> _logger;

    public LogPropertyFilter(ILogger<LogPropertyFilter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executado ANTES da action do controller.
    /// Aqui capturamos e logamos as propriedades marcadas.
    /// </summary>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Check for DisableLogPropertyAttribute on the endpoint metadata and skip if requested
        var disableAttr = context.ActionDescriptor.EndpointMetadata?
            .OfType<DisableLogPropertyAttribute>()
            .FirstOrDefault();

        if (disableAttr != null && disableAttr.Ignore)
        {
            _logger.LogInformation("LogPropertyFilter skipped by DisableLogPropertyAttribute.");
            return;
        }

        // Log do início da execução
        _logger.LogInformation("=== Iniciando interceptação da requisição ===");
        _logger.LogInformation("Controller: {Controller}", context.Controller.GetType().Name);
        _logger.LogInformation("Action: {Action}", context.ActionDescriptor.DisplayName);

        // Lógica de decisão: procuramos no ActionArguments um payload com propriedade int Age
        var accessor = context.HttpContext.RequestServices.GetRequiredService<ISelectedServiceAccessor>();

        int? ageValue = null;
        object? payloadObject = null;

        foreach (var arg in context.ActionArguments)
        {
            if (arg.Value == null) continue;

            var argType = arg.Value.GetType();
            var ageProp = argType.GetProperty("Age", BindingFlags.Public | BindingFlags.Instance);
            if (ageProp != null && ageProp.PropertyType == typeof(int))
            {
                var val = ageProp.GetValue(arg.Value);
                if (val is int i)
                {
                    ageValue = i;
                    payloadObject = arg.Value;
                    break;
                }
            }
        }

        // Decide service com base na idade encontrada (exemplo)
        if (ageValue.HasValue && ageValue.Value < 30)
        {
            accessor.Selected = context.HttpContext.RequestServices.GetRequiredService<BusinessServiceA>();
            _logger.LogInformation("Selecionado BusinessServiceA com base em Age={Age}", ageValue.Value);
        }
        else
        {
            accessor.Selected = context.HttpContext.RequestServices.GetRequiredService<BusinessServiceB>();
            _logger.LogInformation("Selecionado BusinessServiceB (Age={Age})", ageValue.HasValue ? ageValue.Value.ToString() : "null");
        }

        // Itera sobre todos os argumentos passados para a action
        foreach (var argument in context.ActionArguments)
        {
            _logger.LogInformation("Analisando argumento: {ArgumentName} do tipo {ArgumentType}", 
                argument.Key, 
                argument.Value?.GetType().Name ?? "null");

            // Verifica se o argumento não é nulo
            if (argument.Value == null)
            {
                _logger.LogWarning("Argumento {ArgumentName} é nulo", argument.Key);
                continue;
            }

            // Captura as propriedades marcadas com [LogProperty]
            var loggedProperties = ExtractLoggedProperties(argument.Value);

            // Se encontrou propriedades marcadas, loga seus valores
            if (loggedProperties.Any())
            {
                _logger.LogInformation("--- Propriedades marcadas para log encontradas ---");
                
                foreach (var prop in loggedProperties)
                {
                    // Aplica máscara se necessário
                    var valueToLog = prop.MaskValue 
                        ? MaskValue(prop.Value?.ToString()) 
                        : prop.Value?.ToString() ?? "null";

                    _logger.LogInformation("[PROPRIEDADE LOGADA] {PropertyName}: {PropertyValue}", 
                        prop.LogName ?? prop.PropertyName, 
                        valueToLog);
                }
            }
            else
            {
                _logger.LogInformation("Nenhuma propriedade marcada com [LogProperty] encontrada");
            }
        }

        _logger.LogInformation("=== Fim da interceptação ===");
    }

    /// <summary>
    /// Executado DEPOIS da action do controller.
    /// Pode ser usado para logar informações sobre o resultado.
    /// </summary>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        var disableAttr = context.ActionDescriptor.EndpointMetadata?
            .OfType<CustomFilterApi.Attributes.DisableLogPropertyAttribute>()
            .FirstOrDefault();

        if (disableAttr != null && disableAttr.Ignore)
        {
            _logger.LogInformation("LogPropertyFilter.OnActionExecuted skipped by DisableLogPropertyAttribute.");
            return;
        }

        _logger.LogInformation("Action executada. Status: {Status}", 
            context.HttpContext.Response.StatusCode);
    }

    /// <summary>
    /// Extrai propriedades de um objeto que estão marcadas com [LogProperty].
    /// Usa Reflection para inspecionar o objeto em tempo de execução.
    /// </summary>
    private List<LoggedPropertyInfo> ExtractLoggedProperties(object obj)
    {
        var result = new List<LoggedPropertyInfo>();

        // Obtém o tipo do objeto
        var type = obj.GetType();

        // Obtém todas as propriedades públicas do tipo
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Verifica se a propriedade tem o atributo [LogProperty]
            var logAttribute = property.GetCustomAttribute<LogPropertyAttribute>();

            if (logAttribute != null)
            {
                // Obtém o valor da propriedade
                var value = property.GetValue(obj);

                result.Add(new LoggedPropertyInfo
                {
                    PropertyName = property.Name,
                    LogName = logAttribute.LogName,
                    Value = value,
                    MaskValue = logAttribute.MaskValue
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Mascara parte do valor para proteger dados sensíveis.
    /// Exemplo: "senha123" vira "sen***23"
    /// </summary>
    private string MaskValue(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= 4)
            return "***";

        var visibleChars = 2;
        var start = value.Substring(0, visibleChars);
        var end = value.Substring(value.Length - visibleChars);
        var masked = new string('*', value.Length - (visibleChars * 2));

        return $"{start}{masked}{end}";
    }

    /// <summary>
    /// Classe auxiliar para armazenar informações sobre propriedades logadas.
    /// </summary>
    private class LoggedPropertyInfo
    {
        public string PropertyName { get; set; } = string.Empty;
        public string? LogName { get; set; }
        public object? Value { get; set; }
        public bool MaskValue { get; set; }
    }
}
