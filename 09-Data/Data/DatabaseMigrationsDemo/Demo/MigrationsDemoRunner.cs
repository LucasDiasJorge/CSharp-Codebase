using DatabaseMigrationsDemo.Data;
using DatabaseMigrationsDemo.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;

namespace DatabaseMigrationsDemo.Demo;

/// <summary>
/// Cinco cenários: estado inicial, aplicação, efeito do backfill, reversão e o que a
/// reversão custa.
/// </summary>
public sealed class MigrationsDemoRunner
{
    private readonly ILogger<MigrationsDemoRunner> _logger;

    public MigrationsDemoRunner(ILogger<MigrationsDemoRunner> logger)
    {
        _logger = logger;
    }

    public async Task RunAllAsync()
    {
        await RunPendingAsync();
        await RunApplyAsync();
        await RunBackfillResultAsync();
        await RunRevertAsync();
        await RunReapplyAsync();
    }

    private async Task RunPendingAsync()
    {
        Section("1. Banco zerado: tudo pendente");

        await using ShopDbContext database = new ShopDbContext();
        await database.Database.EnsureDeletedAsync();

        IEnumerable<string> pending = await database.Database.GetPendingMigrationsAsync();

        _logger.LogInformation("Migrations pendentes:");
        foreach (string migration in pending)
        {
            _logger.LogInformation("  {Migration}", migration);
        }
    }

    private async Task RunApplyAsync()
    {
        Section("2. Aplicando todas as migrations");

        await using ShopDbContext database = new ShopDbContext();
        await database.Database.MigrateAsync();

        IEnumerable<string> applied = await database.Database.GetAppliedMigrationsAsync();

        _logger.LogInformation("Aplicadas: {Quantidade}", applied.Count());
        foreach (string migration in applied)
        {
            _logger.LogInformation("  {Migration}", migration);
        }

        _logger.LogInformation(
            "O EF registra o que ja rodou na tabela __EFMigrationsHistory — e por isso que aplicar duas vezes nao repete nada.");
    }

    private async Task RunBackfillResultAsync()
    {
        Section("3. O seed veio da migration; o backfill preencheu as colunas novas");

        await using ShopDbContext database = new ShopDbContext();

        List<Customer> customers = await database.Customers.AsNoTracking().OrderBy(customer => customer.Id).ToListAsync();

        foreach (Customer customer in customers)
        {
            _logger.LogInformation(
                "  #{Id} Name=\"{Nome}\" -> FirstName=\"{Primeiro}\" LastName=\"{Ultimo}\" Phone={Telefone}",
                customer.Id,
                customer.Name,
                customer.FirstName,
                customer.LastName,
                customer.Phone ?? "(nulo)");
        }

        _logger.LogInformation(
            "Name continua preenchido: na etapa expand as duas formas coexistem, para a versao anterior da aplicacao seguir funcionando.");
    }

    private async Task RunRevertAsync()
    {
        Section("4. Revertendo para a migration anterior");

        await using ShopDbContext database = new ShopDbContext();

        IMigrator migrator = database.GetService<IMigrator>();

        // Voltar e migrar para um alvo anterior: o EF executa o Down() de tudo o que
        // estiver depois dele.
        await migrator.MigrateAsync("AddCustomerPhone");

        IEnumerable<string> applied = await database.Database.GetAppliedMigrationsAsync();
        IEnumerable<string> pending = await database.Database.GetPendingMigrationsAsync();

        _logger.LogInformation("Aplicadas agora: {Aplicadas}", applied.Count());
        _logger.LogWarning("Pendente de novo: {Pendente}", string.Join(", ", pending));

        // As colunas sumiram — e com elas os dados que estavam ali.
        List<string> columns = await database.Database
            .SqlQueryRaw<string>("SELECT name AS Value FROM pragma_table_info('Customers')")
            .ToListAsync();

        _logger.LogWarning(
            "Colunas de Customers apos o Down: [{Colunas}] — FirstName e LastName foram removidas, junto com o que o backfill tinha escrito.",
            string.Join(", ", columns));
    }

    private async Task RunReapplyAsync()
    {
        Section("5. Reaplicando: o backfill roda de novo");

        await using ShopDbContext database = new ShopDbContext();
        await database.Database.MigrateAsync();

        List<Customer> customers = await database.Customers.AsNoTracking().OrderBy(customer => customer.Id).ToListAsync();

        foreach (Customer customer in customers)
        {
            _logger.LogInformation("  #{Id} FirstName=\"{Primeiro}\" LastName=\"{Ultimo}\"", customer.Id, customer.FirstName, customer.LastName);
        }

        _logger.LogInformation(
            "Os dados voltaram porque a origem (Name) ainda existia. Se a etapa 'contract' ja tivesse removido Name, o Down nao teria de onde reconstruir.");
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
