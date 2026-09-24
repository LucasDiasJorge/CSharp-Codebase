using ConfigurationOptionsDemo.Demo;
using Microsoft.Extensions.Logging;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

LayeringScenarios layering = new LayeringScenarios(loggerFactory.CreateLogger<LayeringScenarios>());
OptionsScenarios options = new OptionsScenarios(loggerFactory.CreateLogger<OptionsScenarios>());

layering.RunPrecedence();

await options.RunValidationAsync();

using (ReloadableConfigFile file = new ReloadableConfigFile())
{
    await options.RunThreeInterfacesAsync(file);
    await options.RunReloadCallbackAsync(file);
}

layering.RunUserSecrets();

await Task.Delay(200);

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");

/// <summary>
/// Âncora de tipo para <c>AddUserSecrets&lt;Program&gt;</c>, que localiza o
/// <c>UserSecretsId</c> pelo assembly do tipo informado.
/// </summary>
public partial class Program;
