using System.Reflection;
using Acme.TextKit;

// Este programa existe para provar tres coisas sobre o PACOTE (nao sobre o codigo):
// que ele instala, que a API funciona e qual versao foi de fato resolvida.

Console.WriteLine("=== Consumindo o pacote Acme.TextKit ===");

Assembly assembly = typeof(TextKit).Assembly;
string? informationalVersion = assembly
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion;

Console.WriteLine($"  assembly:            {assembly.GetName().Name}");
Console.WriteLine($"  AssemblyVersion:     {assembly.GetName().Version}");
Console.WriteLine($"  InformationalVersion: {informationalVersion}");
Console.WriteLine($"  vindo de:            {Path.GetFileName(assembly.Location)}");

Console.WriteLine();
Console.WriteLine("=== A API do pacote ===");
Console.WriteLine($"  Slugify(\"Olá, Mundo Cruel!\") = \"{TextKit.Slugify("Olá, Mundo Cruel!")}\"");
Console.WriteLine($"  Truncate(\"texto bem longo\", 8) = \"{TextKit.Truncate("texto bem longo", 8)}\"");

// CountWords existe na 1.0.0 e desaparece na compilacao com DefineConstants=BREAKING.
MethodInfo? countWords = typeof(TextKit).GetMethod("CountWords");

Console.WriteLine(
    countWords is null
        ? "  CountWords: AUSENTE nesta versao do pacote"
        : $"  CountWords(\"um dois  tres\") = {TextKit.CountWords("um dois  tres")}");

Console.WriteLine();
Console.WriteLine("Fim.");
