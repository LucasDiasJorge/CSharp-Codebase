using Microsoft.EntityFrameworkCore;
using MoneyStorageApi.Data;
using MoneyStorageApi.Domain;
using MoneyStorageApi.DTOs;
using MoneyStorageApi.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MoneyDb")
    ?? throw new InvalidOperationException("Connection string 'MoneyDb' was not found.");

builder.Services.AddDbContext<MoneyStorageContext>(options =>
{
    options.UseMySQL(connectionString, mysql =>
    {
        mysql.EnableRetryOnFailure(5);
        mysql.CommandTimeout(30);
    });

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<AccountService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var accounts = app.MapGroup("/accounts").WithTags("Accounts");
accounts.MapPost("/", async (CreateAccountRequest request, AccountService service, CancellationToken cancellationToken) =>
{
    try
    {
        var account = await service.CreateAccountAsync(request.OwnerName, request.InitialBalance, cancellationToken);
        return Results.Created($"/accounts/{account.Id}", ToResponse(account));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

accounts.MapGet("/", async (AccountService service, CancellationToken cancellationToken) =>
{
    var accounts = await service.GetAccountsAsync(cancellationToken);
    return Results.Ok(accounts.Select(ToResponse));
});

accounts.MapGet("/{id:guid}", async (Guid id, AccountService service, CancellationToken cancellationToken) =>
{
    var account = await service.GetAccountAsync(id, cancellationToken);
    return account is null ? Results.NotFound() : Results.Ok(ToResponse(account));
});

accounts.MapPost("/{id:guid}/deposit", async (Guid id, MoneyOperationRequest request, AccountService service, CancellationToken cancellationToken) =>
{
    try
    {
        var account = await service.DepositAsync(id, request.Amount, request.Description, cancellationToken);
        return account is null ? Results.NotFound() : Results.Ok(ToResponse(account));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

accounts.MapPost("/{id:guid}/withdraw", async (Guid id, MoneyOperationRequest request, AccountService service, CancellationToken cancellationToken) =>
{
    try
    {
        var account = await service.WithdrawAsync(id, request.Amount, request.Description, cancellationToken);
        return account is null ? Results.NotFound() : Results.Ok(ToResponse(account));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

static AccountResponse ToResponse(Account account)
{
    var orderedMovements = account.Movements
        .OrderByDescending(m => m.OccurredAtUtc)
        .Select(m => new MovementResponse(
            m.Id,
            m.Type.ToString(),
            m.Amount,
            m.Description,
            m.OccurredAtUtc))
        .ToList();

    return new AccountResponse(
        account.Id,
        account.OwnerName,
        account.Balance,
        account.CreatedAtUtc,
        orderedMovements);
}
