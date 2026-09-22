using EfCoreOptimisticConcurrencyDemo.Data;
using EfCoreOptimisticConcurrencyDemo.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace EfCoreOptimisticConcurrencyDemo.Demo;

/// <summary>
/// Seis cenários: a atualização perdida sem token, a detecção com token, as três
/// estratégias de resolução e o retry.
/// </summary>
public sealed class ConcurrencyDemoRunner
{
    private readonly ILogger<ConcurrencyDemoRunner> _logger;

    public ConcurrencyDemoRunner(ILogger<ConcurrencyDemoRunner> logger)
    {
        _logger = logger;
    }

    public async Task RunAllAsync()
    {
        await SeedAsync();
        await RunLostUpdateAsync();
        await RunConflictDetectedAsync();
        await RunStoreWinsAsync();
        await RunClientWinsAsync();
        await RunMergeAsync();
        await RunRetryAsync();
    }

    private async Task SeedAsync()
    {
        Section("0. Recriando o banco");

        await using ShopDbContext database = new ShopDbContext();
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        database.Products.Add(new Product { Name = "Teclado", Price = 300m, Stock = 10 });
        database.UnguardedProducts.Add(new UnguardedProduct { Name = "Teclado", Price = 300m, Stock = 10 });

        await database.SaveChangesAsync();

        _logger.LogInformation("Produto criado: preco 300, estoque 10.");
    }

    private async Task RunLostUpdateAsync()
    {
        Section("1. SEM token: a atualizacao perdida, sem erro nenhum");

        // Dois contextos = dois "usuarios" que leram o mesmo registro.
        await using ShopDbContext userA = new ShopDbContext();
        await using ShopDbContext userB = new ShopDbContext();

        UnguardedProduct productA = await userA.UnguardedProducts.FirstAsync();
        UnguardedProduct productB = await userB.UnguardedProducts.FirstAsync();

        _logger.LogInformation("A e B leem o mesmo estoque: {Estoque}.", productA.Stock);

        // Os dois fazem read-modify-write sobre o MESMO campo: A separa 2 unidades,
        // B separa 3. O resultado correto seria 10 - 2 - 3 = 5.
        productA.Stock -= 2;
        await userA.SaveChangesAsync();
        _logger.LogInformation("A separou 2 unidades e gravou estoque {Estoque}.", productA.Stock);

        productB.Stock -= 3;
        await userB.SaveChangesAsync();
        _logger.LogInformation("B separou 3 unidades e gravou estoque {Estoque}.", productB.Stock);

        await using ShopDbContext check = new ShopDbContext();
        UnguardedProduct final = await check.UnguardedProducts.FirstAsync();

        _logger.LogWarning(
            "Estoque final: {Estoque}. O correto seria 5 — a baixa de A se perdeu, e nada acusou o problema.",
            final.Stock);

        _logger.LogInformation(
            "Repare que o conflito exigiu os dois mexerem no MESMO campo: o EF Core gera UPDATE apenas das colunas alteradas, entao alteracoes em campos diferentes nao se atropelam.");
    }

    private async Task RunConflictDetectedAsync()
    {
        Section("2. COM token: o conflito e detectado");

        await using ShopDbContext userA = new ShopDbContext();
        await using ShopDbContext userB = new ShopDbContext();

        Product productA = await userA.Products.FirstAsync();
        Product productB = await userB.Products.FirstAsync();

        _logger.LogInformation("A e B leem a versao {Versao}.", productA.Version);

        productA.Price = 250m;
        await userA.SaveChangesAsync();
        _logger.LogInformation("A gravou. Versao agora: {Versao}.", productA.Version);

        productB.Stock = 42;

        try
        {
            await userB.SaveChangesAsync();
            _logger.LogError("B gravou sem conflito — isso NAO deveria acontecer.");
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning(
                "B foi rejeitado: o UPDATE procurava a versao {Versao}, que ja nao existe. Nenhuma linha afetada.",
                productB.Version);
        }
    }

    private async Task RunStoreWinsAsync()
    {
        Section("3. Resolucao: o banco vence (descarta minhas alteracoes)");

        await ResetProductAsync();

        await using ShopDbContext userA = new ShopDbContext();
        await using ShopDbContext userB = new ShopDbContext();

        Product productA = await userA.Products.FirstAsync();
        Product productB = await userB.Products.FirstAsync();

        productA.Price = 199m;
        await userA.SaveChangesAsync();

        productB.Price = 275m;

        try
        {
            await userB.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            EntityEntry entry = exception.Entries[0];
            PropertyValues? databaseValues = await entry.GetDatabaseValuesAsync();

            // Store wins: recarrega os valores do banco e abandona o que eu tinha.
            entry.OriginalValues.SetValues(databaseValues!);
            entry.CurrentValues.SetValues(databaseValues!);

            _logger.LogInformation(
                "Banco vence: B descartou o preco 275 e ficou com {Preco} — o que A gravou.",
                ((Product)entry.Entity).Price);
        }
    }

    private async Task RunClientWinsAsync()
    {
        Section("4. Resolucao: o cliente vence (sobrescreve)");

        await ResetProductAsync();

        await using ShopDbContext userA = new ShopDbContext();
        await using ShopDbContext userB = new ShopDbContext();

        Product productA = await userA.Products.FirstAsync();
        Product productB = await userB.Products.FirstAsync();

        productA.Price = 199m;
        await userA.SaveChangesAsync();

        productB.Price = 275m;

        try
        {
            await userB.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            EntityEntry entry = exception.Entries[0];
            PropertyValues? databaseValues = await entry.GetDatabaseValuesAsync();

            // Client wins: aceita a versao atual do banco como base e regrava por cima.
            entry.OriginalValues.SetValues(databaseValues!);
            await userB.SaveChangesAsync();

            _logger.LogWarning(
                "Cliente vence: B regravou preco {Preco}, sobrescrevendo o valor de A. So faz sentido quando a alteracao de B e mais autorizada.",
                productB.Price);
        }
    }

    private async Task RunMergeAsync()
    {
        Section("5. Resolucao: mesclar (cada um alterou um campo diferente)");

        await ResetProductAsync();

        await using ShopDbContext userA = new ShopDbContext();
        await using ShopDbContext userB = new ShopDbContext();

        Product productA = await userA.Products.FirstAsync();
        Product productB = await userB.Products.FirstAsync();

        productA.Price = 199m;
        await userA.SaveChangesAsync();
        _logger.LogInformation("A alterou o PRECO para 199.");

        productB.Stock = 77;

        try
        {
            await userB.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            EntityEntry entry = exception.Entries[0];
            PropertyValues? databaseValues = await entry.GetDatabaseValuesAsync();
            PropertyValues currentValues = entry.CurrentValues;

            // Merge campo a campo: mantem o que EU mudei, aceita o resto do banco.
            foreach (Microsoft.EntityFrameworkCore.Metadata.IProperty property in currentValues.Properties)
            {
                object? proposed = currentValues[property];
                object? original = entry.OriginalValues[property];
                object? fromDatabase = databaseValues![property];

                // So mantem o valor proposto se EU realmente o alterei.
                if (Equals(proposed, original))
                {
                    currentValues[property] = fromDatabase;
                }
            }

            entry.OriginalValues.SetValues(databaseValues!);
            await userB.SaveChangesAsync();

            _logger.LogInformation(
                "Mesclado: preco {Preco} (de A) e estoque {Estoque} (de B). Nenhuma alteracao se perdeu.",
                productB.Price,
                productB.Stock);
        }
    }

    private async Task RunRetryAsync()
    {
        Section("6. Retry: reler e reaplicar a intencao");

        await ResetProductAsync();

        // A intencao de B nao e "estoque = 42", e sim "tirar 3 do estoque". Reaplicar
        // a intencao sobre o valor atual e o que torna o retry correto — repetir o
        // valor absoluto reintroduziria a atualizacao perdida.
        const int quantityToRemove = 3;
        const int maxAttempts = 3;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            await using ShopDbContext context = new ShopDbContext();
            Product product = await context.Products.FirstAsync();

            if (attempt == 1)
            {
                // Simula outro processo gravando entre a leitura e a gravacao de B.
                await using ShopDbContext interference = new ShopDbContext();
                Product other = await interference.Products.FirstAsync();
                other.Price = 189m;
                await interference.SaveChangesAsync();

                _logger.LogInformation("Outro processo gravou preco 189 no meio do caminho.");
            }

            product.Stock -= quantityToRemove;

            try
            {
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "Tentativa {Tentativa}: gravou. Estoque final {Estoque}, preco {Preco}.",
                    attempt,
                    product.Stock,
                    product.Price);

                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Tentativa {Tentativa} falhou por conflito; relendo e reaplicando a intencao.", attempt);
            }
        }

        _logger.LogError("Desistiu apos {Tentativas} tentativas.", maxAttempts);
    }

    private static async Task ResetProductAsync()
    {
        await using ShopDbContext database = new ShopDbContext();
        Product product = await database.Products.FirstAsync();

        product.Price = 300m;
        product.Stock = 10;

        await database.SaveChangesAsync();
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
