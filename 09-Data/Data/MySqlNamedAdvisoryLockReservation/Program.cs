using System;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace MySqlNamedAdvisoryLockReservation;

public static class Program
{
    private const string ConnectionStringEnvironmentVariable = "MYSQL_RESERVATION_CONNECTION";

    public static async Task Main(string[] args)
    {
        string? connectionString = args.Length > 0
            ? args[0]
            : Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);

        ReservationRequest firstRequest = new("ROOM-101", 2, "atendente-a");
        ReservationRequest secondRequest = new("ROOM-101", 2, "atendente-b");

        Console.WriteLine("MySQL Named Advisory Lock Reservation");
        Console.WriteLine();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            PrintDryRun(firstRequest);
            return;
        }

        CancellationToken cancellationToken = CancellationToken.None;
        InventoryReservationService service = new(connectionString);

        await service.EnsureSchemaAsync(cancellationToken);

        ReservationResult firstResult = await service.TryReserveAsync(firstRequest, cancellationToken);
        ReservationResult secondResult = await service.TryReserveAsync(secondRequest, cancellationToken);

        PrintResult(firstResult);
        PrintResult(secondResult);
    }

    private static void PrintDryRun(ReservationRequest request)
    {
        string lockName = InventoryReservationService.BuildLockName(request.Sku);

        Console.WriteLine("Nenhuma connection string foi informada.");
        Console.WriteLine("Defina MYSQL_RESERVATION_CONNECTION ou passe a connection string como primeiro argumento.");
        Console.WriteLine();
        Console.WriteLine("Fluxo demonstrado pelo sample:");
        Console.WriteLine($"1. SELECT GET_LOCK('{lockName}', 10);");
        Console.WriteLine("2. BEGIN TRANSACTION;");
        Console.WriteLine("3. SELECT available_quantity ... FOR UPDATE;");
        Console.WriteLine("4. UPDATE reservation_inventory e INSERT reservation_orders;");
        Console.WriteLine($"5. SELECT RELEASE_LOCK('{lockName}');");
    }

    private static void PrintResult(ReservationResult result)
    {
        string status = result.Succeeded ? "CONFIRMADA" : "RECUSADA";
        Console.WriteLine($"{status}: {result.Message}");
        Console.WriteLine($"SKU: {result.Sku}");
        Console.WriteLine($"Quantidade: {result.Quantity}");
        Console.WriteLine($"Saldo restante: {result.RemainingQuantity}");
        Console.WriteLine($"Reserva: {result.ReservationId}");
        Console.WriteLine();
    }
}

public sealed class InventoryReservationService
{
    private const int LockTimeoutSeconds = 10;
    private readonly string connectionString;

    public InventoryReservationService(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public static string BuildLockName(string sku)
    {
        string normalizedSku = sku.Trim().ToUpperInvariant();
        return "reservation:sku:" + normalizedSku;
    }

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await ExecuteNonQueryAsync(
            connection,
            null,
            """
            CREATE TABLE IF NOT EXISTS reservation_inventory (
                sku VARCHAR(64) NOT NULL PRIMARY KEY,
                available_quantity INT NOT NULL,
                updated_at_utc TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)
            ) ENGINE=InnoDB;
            """,
            cancellationToken);

        await ExecuteNonQueryAsync(
            connection,
            null,
            """
            CREATE TABLE IF NOT EXISTS reservation_orders (
                reservation_id CHAR(32) NOT NULL PRIMARY KEY,
                sku VARCHAR(64) NOT NULL,
                quantity INT NOT NULL,
                requested_by VARCHAR(100) NOT NULL,
                status VARCHAR(20) NOT NULL,
                created_at_utc TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
            ) ENGINE=InnoDB;
            """,
            cancellationToken);

        await ExecuteNonQueryAsync(
            connection,
            null,
            """
            INSERT INTO reservation_inventory (sku, available_quantity)
            VALUES ('ROOM-101', 3)
            ON DUPLICATE KEY UPDATE available_quantity = 3;
            """,
            cancellationToken);
    }

    public async Task<ReservationResult> TryReserveAsync(
        ReservationRequest request,
        CancellationToken cancellationToken)
    {
        string lockName = BuildLockName(request.Sku);

        await using MySqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        // MySQL named locks are bound to the connection, so the critical section uses this same connection.
        await using MySqlNamedAdvisoryLock advisoryLock =
            await MySqlNamedAdvisoryLock.AcquireAsync(connection, lockName, LockTimeoutSeconds, cancellationToken);

        await using MySqlTransaction transaction =
            await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            int availableQuantity = await ReadAvailableQuantityAsync(
                connection,
                transaction,
                request.Sku,
                cancellationToken);

            if (availableQuantity < request.Quantity)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ReservationResult.Failure(
                    request.Sku,
                    request.Quantity,
                    availableQuantity,
                    "estoque insuficiente dentro da secao critica protegida pelo lock nomeado");
            }

            int remainingQuantity = availableQuantity - request.Quantity;
            string reservationId = Guid.NewGuid().ToString("N");

            await DecreaseInventoryAsync(connection, transaction, request, cancellationToken);
            await InsertReservationAsync(connection, transaction, request, reservationId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ReservationResult.Success(
                reservationId,
                request.Sku,
                request.Quantity,
                remainingQuantity,
                "reserva persistida de forma atomica");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<int> ReadAvailableQuantityAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string sku,
        CancellationToken cancellationToken)
    {
        using MySqlCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT available_quantity
            FROM reservation_inventory
            WHERE sku = @sku
            FOR UPDATE;
            """;
        command.Parameters.AddWithValue("@sku", sku);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        if (result is null || result == DBNull.Value)
        {
            throw new InvalidOperationException("SKU nao encontrado no estoque demo.");
        }

        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    private static async Task DecreaseInventoryAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ReservationRequest request,
        CancellationToken cancellationToken)
    {
        using MySqlCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE reservation_inventory
            SET available_quantity = available_quantity - @quantity
            WHERE sku = @sku;
            """;
        command.Parameters.AddWithValue("@sku", request.Sku);
        command.Parameters.AddWithValue("@quantity", request.Quantity);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affectedRows != 1)
        {
            throw new InvalidOperationException("Atualizacao de estoque nao afetou exatamente uma linha.");
        }
    }

    private static async Task InsertReservationAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ReservationRequest request,
        string reservationId,
        CancellationToken cancellationToken)
    {
        using MySqlCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO reservation_orders (
                reservation_id,
                sku,
                quantity,
                requested_by,
                status,
                created_at_utc
            )
            VALUES (
                @reservationId,
                @sku,
                @quantity,
                @requestedBy,
                'Reserved',
                UTC_TIMESTAMP(6)
            );
            """;
        command.Parameters.AddWithValue("@reservationId", reservationId);
        command.Parameters.AddWithValue("@sku", request.Sku);
        command.Parameters.AddWithValue("@quantity", request.Quantity);
        command.Parameters.AddWithValue("@requestedBy", request.RequestedBy);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affectedRows != 1)
        {
            throw new InvalidOperationException("Insercao de reserva nao afetou exatamente uma linha.");
        }
    }

    private static async Task ExecuteNonQueryAsync(
        MySqlConnection connection,
        MySqlTransaction? transaction,
        string commandText,
        CancellationToken cancellationToken)
    {
        using MySqlCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

public sealed class MySqlNamedAdvisoryLock : IAsyncDisposable
{
    private readonly MySqlConnection connection;
    private readonly string lockName;
    private bool isReleased;

    private MySqlNamedAdvisoryLock(MySqlConnection connection, string lockName)
    {
        this.connection = connection;
        this.lockName = lockName;
    }

    public static async Task<MySqlNamedAdvisoryLock> AcquireAsync(
        MySqlConnection connection,
        string lockName,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        using MySqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT GET_LOCK(@lockName, @timeoutSeconds);";
        command.Parameters.AddWithValue("@lockName", lockName);
        command.Parameters.AddWithValue("@timeoutSeconds", timeoutSeconds);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        int lockResult = ConvertLockResult(result, "GET_LOCK");

        if (lockResult == 0)
        {
            throw new TimeoutException("Timeout ao aguardar o advisory lock nomeado do MySQL.");
        }

        if (lockResult != 1)
        {
            throw new InvalidOperationException("MySQL retornou um resultado inesperado para GET_LOCK.");
        }

        return new MySqlNamedAdvisoryLock(connection, lockName);
    }

    public async ValueTask DisposeAsync()
    {
        if (isReleased)
        {
            return;
        }

        using MySqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT RELEASE_LOCK(@lockName);";
        command.Parameters.AddWithValue("@lockName", lockName);

        object? result = await command.ExecuteScalarAsync(CancellationToken.None);
        int releaseResult = ConvertLockResult(result, "RELEASE_LOCK");

        if (releaseResult != 1)
        {
            throw new InvalidOperationException("MySQL nao confirmou a liberacao do advisory lock nomeado.");
        }

        isReleased = true;
    }

    private static int ConvertLockResult(object? result, string functionName)
    {
        if (result is null || result == DBNull.Value)
        {
            throw new InvalidOperationException(functionName + " retornou NULL.");
        }

        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }
}

public sealed record ReservationRequest(string Sku, int Quantity, string RequestedBy);

public sealed record ReservationResult(
    bool Succeeded,
    string ReservationId,
    string Sku,
    int Quantity,
    int RemainingQuantity,
    string Message)
{
    public static ReservationResult Success(
        string reservationId,
        string sku,
        int quantity,
        int remainingQuantity,
        string message)
    {
        return new ReservationResult(true, reservationId, sku, quantity, remainingQuantity, message);
    }

    public static ReservationResult Failure(
        string sku,
        int quantity,
        int remainingQuantity,
        string message)
    {
        return new ReservationResult(false, "-", sku, quantity, remainingQuantity, message);
    }
}
