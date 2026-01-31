// =============================================================================
// CIRCUIT BREAKER - Implementação Didática em C# (arquivo mínimo: apenas Program)
// =============================================================================
using System;
using System.Threading;

namespace CircuitBreakerDemo;

class Program
{
    static void Main()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      CIRCUIT BREAKER - Demonstração Didática             ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Cria um Circuit Breaker:
        // - Abre após 3 erros consecutivos
        // - Fica aberto por 5 segundos
        CircuitBreaker circuitBreaker = new CircuitBreaker(limiteErros: 3, segundosEspera: 5);

        // Simula um serviço instável
        ServicoInstavel servicoInstavel = new ServicoInstavel();

        // ===========================================================
        // DEMONSTRAÇÃO: Veja o Circuit Breaker em ação!
        // ===========================================================

        for (int i = 1; i <= 10; i++)
        {
            Console.WriteLine($"\n═══ Tentativa #{i} ═══");

            try
            {
                // Tenta chamar o serviço através do Circuit Breaker
                string resultado = circuitBreaker.Executar(
                    () => servicoInstavel.ChamarAPI(),
                    nome: "API Externa"
                );

                Console.WriteLine($"💬 Resposta: {resultado}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Exceção capturada: {ex.Message}");
            }

            // Pequena pausa entre chamadas
            Thread.Sleep(1000);
        }

        Console.WriteLine("\n\n✅ Demonstração concluída!");
        Console.WriteLine("\nO que aconteceu:");
        Console.WriteLine("1. Primeiras chamadas falharam (serviço instável)");
        Console.WriteLine("2. Após 3 falhas, o circuito ABRIU (proteção)");
        Console.WriteLine("3. Chamadas seguintes foram BLOQUEADAS imediatamente");
        Console.WriteLine("4. Após 5 segundos, entrou em MEIO-ABERTO (teste)");
        Console.WriteLine("5. Se tiver sucesso, volta ao FECHADO (normal)");

        Console.WriteLine("\n📖 Veja o README.md para mais detalhes e referências!");
    }
}
