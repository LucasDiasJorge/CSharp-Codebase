using IdempotencyCacheApi.Models;
using IdempotencyCacheApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdempotencyCacheApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PaymentsController : ControllerBase
{
    private const string IdempotencyHeaderName = "Idempotency-Key";

    private readonly IIdempotencyService _idempotencyService;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IIdempotencyService idempotencyService,
        IPaymentProcessor paymentProcessor,
        ILogger<PaymentsController> logger)
    {
        _idempotencyService = idempotencyService;
        _paymentProcessor = paymentProcessor;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentResponse>> CreateAsync(
        [FromHeader(Name = IdempotencyHeaderName)] string? idempotencyKey,
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            ProblemDetails badRequest = new ProblemDetails
            {
                Title = "Idempotency key is required.",
                Detail = "Send Idempotency-Key in the request header.",
                Status = StatusCodes.Status400BadRequest
            };

            return BadRequest(badRequest);
        }

        IdempotencyExecutionResult executionResult = await _idempotencyService.ExecuteAsync(
            idempotencyKey,
            request,
            token => _paymentProcessor.ProcessAsync(request, token),
            cancellationToken);

        if (executionResult.Status == IdempotencyExecutionStatus.Conflict)
        {
            ProblemDetails conflict = new ProblemDetails
            {
                Title = "Idempotency conflict.",
                Detail = executionResult.ErrorMessage,
                Status = StatusCodes.Status409Conflict
            };

            return Conflict(conflict);
        }

        if (executionResult.Response is null)
        {
            ProblemDetails unknownError = new ProblemDetails
            {
                Title = "Unexpected idempotency execution result.",
                Status = StatusCodes.Status500InternalServerError
            };

            return StatusCode(StatusCodes.Status500InternalServerError, unknownError);
        }

        if (executionResult.Status == IdempotencyExecutionStatus.Replay)
        {
            Response.Headers["X-Idempotency-Replay"] = "true";
            _logger.LogInformation("Cached replay returned for key {IdempotencyKey}.", idempotencyKey);
        }
        else
        {
            Response.Headers["X-Idempotency-Replay"] = "false";
        }

        return Ok(executionResult.Response);
    }

    [HttpDelete("cache/{idempotencyKey}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult InvalidateCache(string idempotencyKey)
    {
        bool invalidated = _idempotencyService.Invalidate(idempotencyKey);

        if (!invalidated)
        {
            ProblemDetails notFound = new ProblemDetails
            {
                Title = "Idempotency key not found in cache.",
                Detail = "The key was not found or has already expired.",
                Status = StatusCodes.Status404NotFound
            };

            return NotFound(notFound);
        }

        return NoContent();
    }
}
