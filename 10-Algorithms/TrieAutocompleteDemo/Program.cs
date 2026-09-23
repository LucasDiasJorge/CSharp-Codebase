using Microsoft.Extensions.Logging;
using TrieAutocompleteDemo.Demo;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("TrieAutocompleteDemo - arvore de prefixos: autocomplete, custo por comprimento e o preco em memoria");

TrieDemoRunner runner = new TrieDemoRunner(loggerFactory.CreateLogger<TrieDemoRunner>());
runner.RunAll();

await Task.Delay(200);
Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
