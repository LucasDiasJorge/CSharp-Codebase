using System;
using Microsoft.Extensions.Logging;

namespace CustomFilterApi.Services;

public class BusinessServiceB : IBusinessService
{
    private readonly ILogger<BusinessServiceB> _logger;

    public BusinessServiceB(ILogger<BusinessServiceB> logger)
    {
        _logger = logger;
    }

    public string Execute(object payload)
    {
        _logger.LogInformation("BusinessServiceB executando lógica para payload do tipo {Type}", payload?.GetType().Name ?? "null");
        return "BusinessServiceB result";
    }
}
