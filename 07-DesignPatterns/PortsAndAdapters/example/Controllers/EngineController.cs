using Microsoft.AspNetCore.Mvc;
using PortsAndAdapters.app_core;

namespace example.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EngineController : ControllerBase
{
    private readonly EngineFactory _engineFactory;

    public EngineController(EngineFactory engineFactory)
    {
        _engineFactory = engineFactory;
    }

    [HttpGet("available")]
    public ActionResult<IReadOnlyCollection<string>> GetAvailableEngines()
    {
        IReadOnlyCollection<string> availableEngines = _engineFactory.GetAvailableEngines();
        return Ok(availableEngines);
    }

    [HttpPost("execute")]
    public async Task<ActionResult<EngineExecutionResult>> Execute([FromBody] ExecuteEngineRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EngineName))
        {
            return BadRequest("EngineName is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Command))
        {
            return BadRequest("Command is required.");
        }

        try
        {
            EngineExecutionResult result = await _engineFactory.ExecuteCommandAsync(request.EngineName, request.Command, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}

public sealed class ExecuteEngineRequest
{
    public string EngineName { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}
