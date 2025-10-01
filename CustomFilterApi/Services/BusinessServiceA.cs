using System;
using Microsoft.Extensions.Logging;

namespace CustomFilterApi.Services;

public class BusinessServiceA : IBusinessService
{
    private readonly ILogger<BusinessServiceA> _logger;

    public BusinessServiceA(ILogger<BusinessServiceA> logger)
    {
        _logger = logger;
    }

    public string Execute(object payload)
    {
        _logger.LogInformation("BusinessServiceA executando lógica para payload do tipo {Type}", payload?.GetType().Name ?? "null");
        return "BusinessServiceA result";
    }
}
