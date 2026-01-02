namespace SagaPattern.Core;

/// <summary>
/// Implementação base do orquestrador de Saga
/// Executa passos em sequência e compensa em caso de falha
/// </summary>
public abstract class SagaOrchestrator<TContext> : ISagaOrchestrator<TContext>
{
    private readonly List<ISagaStep<TContext>> _steps = [];
    private readonly List<string> _executedSteps = [];
    private readonly List<string> _compensatedSteps = [];

    protected void AddStep(ISagaStep<TContext> step)
    {
        _steps.Add(step);
    }

    public async Task<SagaResult> ExecuteAsync(TContext context, CancellationToken ct = default)
    {
        _executedSteps.Clear();
        _compensatedSteps.Clear();

        Console.WriteLine($"\n  [Saga] Iniciando execução com {_steps.Count} passos");

        foreach (var step in _steps)
        {
            Console.WriteLine($"  [Saga] Executando: {step.Name}");

            try
            {
                var success = await step.ExecuteAsync(context, ct);

                if (!success)
                {
                    Console.WriteLine($"  [Saga] ❌ Falha em: {step.Name}");
                    return await CompensateAsync(context, $"Falha no passo: {step.Name}", ct);
                }

                _executedSteps.Add(step.Name);
                Console.WriteLine($"  [Saga] ✅ Sucesso: {step.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [Saga] ❌ Exceção em {step.Name}: {ex.Message}");
                return await CompensateAsync(context, ex.Message, ct);
            }
        }

        Console.WriteLine("  [Saga] ✅ Saga completada com sucesso!");
        return SagaResult.Success(_executedSteps);
    }

    private async Task<SagaResult> CompensateAsync(TContext context, string error, CancellationToken ct)
    {
        Console.WriteLine("\n  [Saga] 🔄 Iniciando compensação...");

        // Compensar na ordem reversa
        var stepsToCompensate = _steps
            .Take(_executedSteps.Count)
            .Reverse()
            .ToList();

        foreach (var step in stepsToCompensate)
        {
            try
            {
                Console.WriteLine($"  [Saga] Compensando: {step.Name}");
                await step.CompensateAsync(context, ct);
                _compensatedSteps.Add(step.Name);
                Console.WriteLine($"  [Saga] ↩️ Compensado: {step.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [Saga] ⚠️ Falha na compensação de {step.Name}: {ex.Message}");
                return SagaResult.CompensationFailed(
                    $"Compensação falhou em {step.Name}: {ex.Message}",
                    _executedSteps,
                    _compensatedSteps
                );
            }
        }

        Console.WriteLine("  [Saga] ↩️ Compensação completa");
        return SagaResult.Failure(error, _executedSteps, _compensatedSteps);
    }
}
