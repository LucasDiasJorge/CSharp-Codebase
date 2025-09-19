using System.Collections.Concurrent;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

// Small demo app showing atomic operations patterns for backend systems
Console.WriteLine("Atomic Operations Demo\n");

await RunInterlockedDemo();
await RunLockDemo();
await RunConcurrentDictionaryDemo();
await RunEfCoreTransactionDemo();
await RunEfCoreOptimisticConcurrencyDemo();
await RunRedisDistributedLockDemo();

Console.WriteLine("Demos completos.");

// -------------------- Interlocked demo --------------------
static async Task RunInterlockedDemo()
{
    Console.WriteLine("1) Interlocked (atomic primitives)");

    var counter = 0;
    var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
    {
        for (int i = 0; i < 1000; i++)
        {
            Interlocked.Increment(ref counter);
        }
    })).ToArray();

    await Task.WhenAll(tasks);
    Console.WriteLine($"Valor esperado: 100000, Valor obtido: {counter}\n");
}

// -------------------- lock demo --------------------
static async Task RunLockDemo()
{
    Console.WriteLine("2) lock (monitor) para seções críticas)");

    var counter = 0;
    var sync = new object();

    var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
    {
        for (int i = 0; i < 1000; i++)
        {
            lock (sync)
            {
                counter++;
            }
        }
    })).ToArray();

    await Task.WhenAll(tasks);
    Console.WriteLine($"Valor esperado: 100000, Valor obtido: {counter}\n");
}

// -------------------- ConcurrentDictionary demo --------------------
static async Task RunConcurrentDictionaryDemo()
{
    Console.WriteLine("3) ConcurrentDictionary para updates atômicos por chave");

    var dict = new ConcurrentDictionary<string,int>();
    var keys = Enumerable.Range(0, 100).Select(i => $"k{i}").ToArray();

    var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
    {
        var rnd = new Random();
        for (int i = 0; i < 1000; i++)
        {
            var k = keys[rnd.Next(keys.Length)];
            dict.AddOrUpdate(k, 1, (_, old) => old + 1);
        }
    })).ToArray();

    await Task.WhenAll(tasks);
    Console.WriteLine($"Total de eventos inseridos: {dict.Values.Sum()} (esperado 100000)\n");
}

// -------------------- EF Core transaction demo --------------------
static async Task RunEfCoreTransactionDemo()
{
    Console.WriteLine("4) Transações com EF Core (Sqlite in-memory)");

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite("Data Source=atomic_demo.db")
        .Options;

    using (var ctx = new AppDbContext(options))
    {
        await ctx.Database.EnsureDeletedAsync();
        await ctx.Database.EnsureCreatedAsync();

        ctx.Accounts.Add(new Account { Id = 1, Balance = 1000 });
        ctx.Accounts.Add(new Account { Id = 2, Balance = 1000 });
        await ctx.SaveChangesAsync();
    }

    using (var ctx = new AppDbContext(options))
    {
        using var tx = await ctx.Database.BeginTransactionAsync();
        try
        {
            var from = await ctx.Accounts.FindAsync(1);
            var to = await ctx.Accounts.FindAsync(2);

            from!.Balance -= 100;
            to!.Balance += 100;

            await ctx.SaveChangesAsync();
            await tx.CommitAsync();

            Console.WriteLine("Transferência com transação concluída com sucesso.\n");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            Console.WriteLine($"Falha na transação: {ex.Message}\n");
        }
    }
}

// -------------------- EF Core optimistic concurrency demo --------------------
static async Task RunEfCoreOptimisticConcurrencyDemo()
{
    Console.WriteLine("5) Concorrência otimista com EF Core (RowVersion)");

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite("Data Source=atomic_demo.db")
        .Options;

    using var t1 = new AppDbContext(options);
    using var t2 = new AppDbContext(options);

    var a1 = await t1.Accounts.FindAsync(1);
    var a2 = await t2.Accounts.FindAsync(1);

    a1!.Balance -= 200;
    await t1.SaveChangesAsync();

    a2!.Balance -= 300;
    try
    {
        await t2.SaveChangesAsync();
        Console.WriteLine("t2 salvou sem conflito (inesperado)");
    }
    catch (DbUpdateConcurrencyException)
    {
        Console.WriteLine("Conflito detectado em t2 — implementar retry ou fallback\n");
    }
}

// -------------------- Redis distributed lock demo --------------------
static async Task RunRedisDistributedLockDemo()
{
    Console.WriteLine("6) Lock distribuído simples com Redis (SET NX + expiry)");

    var mux = ConnectionMultiplexer.Connect("localhost:6379");
    var db = mux.GetDatabase();
    var lockKey = "lock:resource-1";
    var token = Guid.NewGuid().ToString();
    var acquired = await db.StringSetAsync(lockKey, token, TimeSpan.FromSeconds(10), When.NotExists);

    if (acquired)
    {
        Console.WriteLine("Lock adquirido, executando operação crítica...");
        // simula trabalho
        await Task.Delay(500);

        // liberação segura: só remove se token bater
        var script = @"if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
        await db.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { token });
        Console.WriteLine("Lock liberado.\n");
    }
    else
    {
        Console.WriteLine("Não foi possível adquirir o lock (outro processo está segurando).\n");
    }

    mux.Dispose();
}

// -------------------- EF Core model --------------------
class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(b =>
        {
            b.Property<byte[]>("RowVersion").IsRowVersion();
        });
    }
}

class Account
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
}
