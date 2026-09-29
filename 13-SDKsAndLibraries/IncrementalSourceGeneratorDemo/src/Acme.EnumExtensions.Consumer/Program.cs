using System.Diagnostics;
using Acme.EnumExtensions;

// A extensao gerada fica no namespace do ENUM, nao no do gerador. Sem este using,
// Shipping.Carrier.ToStringFast() nao compila (CS1929) mesmo tendo sido gerada.
using Shipping;

// Nenhum dos metodos usados abaixo existe neste projeto: todos foram GERADOS em tempo de
// compilacao, a partir do atributo [EnumExtensions] nos enums de Enums.cs.

Console.WriteLine("=== 1. O codigo gerado existe e funciona ===");

OrderStatus status = OrderStatus.AwaitingPayment;

Console.WriteLine($"  status.ToStringFast()  = \"{status.ToStringFast()}\"");
Console.WriteLine($"  status.ToString()      = \"{status.ToString()}\"");
Console.WriteLine($"  valores declarados     = {OrderStatusExtensions.DeclaredValues.Length}");

Console.WriteLine();
Console.WriteLine("=== 2. Funciona para mais de um enum, em namespaces diferentes ===");

Shipping.Carrier carrier = Shipping.Carrier.Correios;

Console.WriteLine($"  carrier.ToStringFast() = \"{carrier.ToStringFast()}\"");
Console.WriteLine($"  Priority.High          = \"{Priority.High.ToStringFast()}\"");

Console.WriteLine();
Console.WriteLine("=== 3. Por que gerar em vez de refletir ===");

const int iterations = 2_000_000;

// Aquecimento: a primeira chamada de Enum.ToString paga a montagem do cache interno.
for (int index = 0; index < 1_000; index++)
{
    _ = status.ToString();
    _ = status.ToStringFast();
}

long beforeReflection = GC.GetTotalAllocatedBytes(precise: true);
Stopwatch watch = Stopwatch.StartNew();

for (int index = 0; index < iterations; index++)
{
    _ = status.ToString();
}

watch.Stop();
long reflectionMs = watch.ElapsedMilliseconds;
long reflectionBytes = GC.GetTotalAllocatedBytes(precise: true) - beforeReflection;

long beforeGenerated = GC.GetTotalAllocatedBytes(precise: true);
watch.Restart();

for (int index = 0; index < iterations; index++)
{
    _ = status.ToStringFast();
}

watch.Stop();
long generatedMs = watch.ElapsedMilliseconds;
long generatedBytes = GC.GetTotalAllocatedBytes(precise: true) - beforeGenerated;

Console.WriteLine($"  {iterations:N0} chamadas:");
Console.WriteLine($"    ToString()     : {reflectionMs,5}ms, {reflectionBytes / 1024.0 / 1024.0,7:N1} MB alocados");
Console.WriteLine($"    ToStringFast() : {generatedMs,5}ms, {generatedBytes / 1024.0 / 1024.0,7:N1} MB alocados");
Console.WriteLine("  O switch gerado devolve uma constante de string: nao reflete e nao aloca.");

Console.WriteLine();
Console.WriteLine("Para LER o codigo gerado: obj/generated/Acme.EnumExtensions.Generator/...");
Console.WriteLine("Fim.");
