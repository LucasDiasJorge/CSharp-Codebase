using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiTenantDataIsolationDemo.Data;
using MultiTenantDataIsolationDemo.Model;

namespace MultiTenantDataIsolationDemo.Demo;

/// <summary>
/// Sete cenários: o filtro funcionando, a atribuição automática, o índice composto e
/// as quatro formas de furar o isolamento.
/// </summary>
public sealed class MultiTenantDemoRunner
{
    private const string TenantA = "acme";
    private const string TenantB = "globex";

    private readonly ILogger<MultiTenantDemoRunner> _logger;

    public MultiTenantDemoRunner(ILogger<MultiTenantDemoRunner> logger)
    {
        _logger = logger;
    }

    public async Task RunAllAsync()
    {
        await SeedAsync();
        await RunFilterWorksAsync();
        await RunAutomaticTenantAsync();
        await RunCompositeIndexAsync();
        await RunLeakRawSqlAsync();
        await RunLeakIgnoreFiltersAsync();
        await RunLeakFindCacheAsync();
        await RunCrossTenantWriteBlockedAsync();
    }

    private async Task SeedAsync()
    {
        Section("0. Recriando o banco com dois tenants");

        TenantContext seedTenant = new TenantContext(TenantA);
        await using BillingDbContext database = new BillingDbContext(seedTenant);

        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        // TenantId explicito no seed, para criar dados dos dois tenants de uma vez.
        database.Invoices.AddRange(
            new Invoice { TenantId = TenantA, Customer = "Ana", Amount = 100m },
            new Invoice { TenantId = TenantA, Customer = "Bruno", Amount = 250m },
            new Invoice { TenantId = TenantB, Customer = "Carla", Amount = 900m },
            new Invoice { TenantId = TenantB, Customer = "Diego", Amount = 1200m });

        await database.SaveChangesAsync();

        _logger.LogInformation("Criadas 2 faturas para '{A}' e 2 para '{B}'.", TenantA, TenantB);
    }

    private async Task RunFilterWorksAsync()
    {
        Section("1. A MESMA consulta, resultados diferentes por tenant");

        foreach (string tenantId in new[] { TenantA, TenantB })
        {
            TenantContext tenant = new TenantContext(tenantId);
            await using BillingDbContext database = new BillingDbContext(tenant);

            List<Invoice> invoices = await database.Invoices.AsNoTracking().ToListAsync();
            decimal total = await database.Invoices.SumAsync(invoice => invoice.Amount);

            _logger.LogInformation(
                "  tenant '{Tenant}': {Quantidade} fatura(s), total {Total:F2} — [{Clientes}]",
                tenantId,
                invoices.Count,
                total,
                string.Join(", ", invoices.Select(invoice => invoice.Customer)));
        }

        _logger.LogInformation("Nenhuma das consultas mencionou TenantId: o filtro global entrou sozinho, inclusive no SUM.");
    }

    private async Task RunAutomaticTenantAsync()
    {
        Section("2. TenantId preenchido automaticamente na inclusao");

        TenantContext tenant = new TenantContext(TenantB);
        await using BillingDbContext database = new BillingDbContext(tenant);

        // Repare: nao atribuimos TenantId.
        Invoice invoice = new Invoice { Customer = "Elena", Amount = 640m };
        database.Invoices.Add(invoice);

        await database.SaveChangesAsync();

        _logger.LogInformation(
            "Fatura de {Cliente} gravada com TenantId='{Tenant}', sem ninguem atribuir. Sem isso, a linha ficaria orfa.",
            invoice.Customer,
            invoice.TenantId);
    }

    private async Task RunCompositeIndexAsync()
    {
        Section("3. Indice composto: TenantId vem primeiro");

        TenantContext tenant = new TenantContext(TenantA);
        await using BillingDbContext database = new BillingDbContext(tenant);

        List<string> indexes = await database.Database
            .SqlQueryRaw<string>("SELECT name AS Value FROM sqlite_master WHERE type='index' AND tbl_name='Invoices'")
            .ToListAsync();

        _logger.LogInformation("Indices em Invoices: [{Indices}]", string.Join(", ", indexes));

        // EXPLAIN QUERY PLAN devolve quatro colunas (id, parent, notused, detail) e o
        // texto do plano esta em 'detail'. SqlQueryRaw<string> leria so a primeira, por
        // isso a leitura e feita pelo ADO direto.
        _logger.LogInformation("Plano com TenantId primeiro:  {Plano}", await ExplainAsync(database, "WHERE TenantId = 'acme' AND Customer = 'Ana'"));
        _logger.LogInformation("Plano so por Customer:        {Plano}", await ExplainAsync(database, "WHERE Customer = 'Ana'"));
        _logger.LogInformation(
            "Como toda consulta filtra por tenant, TenantId precisa ser a PRIMEIRA coluna do indice. Invertido, o indice quase nao seria usado.");
    }

    private async Task RunLeakRawSqlAsync()
    {
        Section("4. VAZAMENTO: SQL cru ignora o filtro global");

        TenantContext tenant = new TenantContext(TenantA);
        await using BillingDbContext database = new BillingDbContext(tenant);

        // FromSqlRaw sem WHERE de tenant: o filtro global do EF nao se aplica a SQL
        // escrito a mao que nao passe pelo LINQ.
        List<Invoice> all = await database.Invoices
            .FromSqlRaw("SELECT Id, TenantId, Customer, Amount FROM Invoices")
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync();

        _logger.LogError(
            "Estando em '{Tenant}', o SQL cru trouxe {Quantidade} faturas de {Tenants} tenants: [{Clientes}]",
            TenantA,
            all.Count,
            all.Select(invoice => invoice.TenantId).Distinct().Count(),
            string.Join(", ", all.Select(invoice => $"{invoice.Customer}/{invoice.TenantId}")));

        _logger.LogWarning("Todo SQL escrito a mao precisa filtrar por tenant explicitamente — o EF nao faz isso por voce.");
    }

    private async Task RunLeakIgnoreFiltersAsync()
    {
        Section("5. VAZAMENTO: IgnoreQueryFilters desliga a protecao");

        TenantContext tenant = new TenantContext(TenantA);
        await using BillingDbContext database = new BillingDbContext(tenant);

        int visible = await database.Invoices.CountAsync();
        int everything = await database.Invoices.IgnoreQueryFilters().CountAsync();

        _logger.LogError(
            "Estando em '{Tenant}': {Visiveis} faturas com o filtro, {Todas} com IgnoreQueryFilters.",
            TenantA,
            visible,
            everything);

        _logger.LogWarning(
            "IgnoreQueryFilters existe para relatorio administrativo e manutencao. Em codigo de aplicacao, cada uso precisa de justificativa — vale proibir por analisador ou revisao.");
    }

    private async Task RunLeakFindCacheAsync()
    {
        Section("6. VAZAMENTO: Find() responde do cache, sem aplicar o filtro");

        // Carrega uma fatura do tenant B...
        TenantContext tenant = new TenantContext(TenantB);
        await using BillingDbContext database = new BillingDbContext(tenant);

        Invoice? fromB = await database.Invoices.FirstAsync();
        int idFromB = fromB.Id;

        _logger.LogInformation("Em '{Tenant}', carregada a fatura #{Id} de {Cliente}.", TenantB, idFromB, fromB.Customer);

        // ...e troca o tenant do MESMO contexto.
        tenant.CurrentTenantId = TenantA;

        Invoice? viaQuery = await database.Invoices.FirstOrDefaultAsync(invoice => invoice.Id == idFromB);
        Invoice? viaFind = await database.Invoices.FindAsync(idFromB);

        _logger.LogInformation("Agora em '{Tenant}': consulta LINQ devolve {Resultado}.", TenantA, viaQuery is null ? "null (correto)" : "a fatura (errado)");

        _logger.LogError(
            "Mas Find() devolveu {Resultado} — ele consulta o change tracker ANTES do banco, e o cache local nao passa pelo filtro global.",
            viaFind is null ? "null" : $"a fatura de {viaFind.Customer}, do tenant '{viaFind.TenantId}'");

        _logger.LogWarning("Nao reutilize o mesmo DbContext entre tenants. Contexto por requisicao, com o tenant fixado na criacao.");
    }

    private async Task RunCrossTenantWriteBlockedAsync()
    {
        Section("7. Gravacao cruzada bloqueada pelo SaveChanges");

        TenantContext tenant = new TenantContext(TenantB);
        await using BillingDbContext database = new BillingDbContext(tenant);

        Invoice invoice = await database.Invoices.FirstAsync();
        tenant.CurrentTenantId = TenantA;

        invoice.Amount = 1m;

        try
        {
            await database.SaveChangesAsync();
            _logger.LogError("Gravou — a protecao falhou.");
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogInformation("Bloqueado: {Mensagem}", exception.Message);
        }
    }

    /// <summary>
    /// Lê a coluna <c>detail</c> do EXPLAIN QUERY PLAN, que é onde o SQLite escreve se
    /// a consulta varreu a tabela ou usou índice.
    /// </summary>
    private static async Task<string> ExplainAsync(BillingDbContext database, string whereClause)
    {
        DbConnection connection = database.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using DbCommand command = connection.CreateCommand();
        command.CommandText = $"EXPLAIN QUERY PLAN SELECT * FROM Invoices {whereClause}";

        await using DbDataReader reader = await command.ExecuteReaderAsync();

        List<string> lines = new List<string>();
        while (await reader.ReadAsync())
        {
            lines.Add(reader.GetString(reader.GetOrdinal("detail")));
        }

        return string.Join(" | ", lines);
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
