using UseCases.Core;
using UseCases.Examples.TransferMoney.DTOs;
using UseCases.Examples.TransferMoney.Entities;
using UseCases.Examples.TransferMoney.Interfaces;

namespace UseCases.Examples.TransferMoney;

/// <summary>
/// Use Case: Transferência de dinheiro entre contas
/// 
/// Este é um exemplo complexo que demonstra:
/// - Transações de banco de dados
/// - Validações de domínio
/// - Regras de negócio (limites, saldo)
/// - Auditoria
/// </summary>
public class TransferMoneyUseCase : IUseCase<TransferMoneyInput, Result<TransferMoneyOutput>>
{
    private readonly IBankAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public TransferMoneyUseCase(
        IBankAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<Result<TransferMoneyOutput>> ExecuteAsync(
        TransferMoneyInput input,
        CancellationToken cancellationToken = default)
    {
        // 1. Validações básicas
        if (input.Amount <= 0)
            return Result<TransferMoneyOutput>.Failure("Valor da transferência deve ser positivo");

        if (input.SourceAccountId == input.DestinationAccountId)
            return Result<TransferMoneyOutput>.Failure("Conta de origem e destino devem ser diferentes");

        // 2. Buscar contas
        var sourceAccount = await _accountRepository.GetByIdAsync(input.SourceAccountId, cancellationToken);
        if (sourceAccount is null)
            return Result<TransferMoneyOutput>.Failure("Conta de origem não encontrada");

        var destinationAccount = await _accountRepository.GetByIdAsync(input.DestinationAccountId, cancellationToken);
        if (destinationAccount is null)
            return Result<TransferMoneyOutput>.Failure("Conta de destino não encontrada");

        // 3. Iniciar transação de banco de dados
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 4. Realizar débito (com validações de domínio)
            var withdrawResult = sourceAccount.Withdraw(input.Amount);
            if (withdrawResult.IsFailure)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return Result<TransferMoneyOutput>.Failure(withdrawResult.Error);
            }

            // 5. Realizar crédito
            var depositResult = destinationAccount.Deposit(input.Amount);
            if (depositResult.IsFailure)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return Result<TransferMoneyOutput>.Failure(depositResult.Error);
            }

            // 6. Persistir alterações
            await _accountRepository.UpdateAsync(sourceAccount, cancellationToken);
            await _accountRepository.UpdateAsync(destinationAccount, cancellationToken);

            // 7. Criar registro de transação
            var transaction = Transaction.Create(
                input.SourceAccountId,
                input.DestinationAccountId,
                input.Amount,
                input.Description
            );
            await _transactionRepository.AddAsync(transaction, cancellationToken);

            // 8. Commit da transação
            await _unitOfWork.CommitAsync(cancellationToken);

            // 9. Registrar auditoria (fora da transação principal)
            await _auditService.LogTransferAsync(
                transaction.Id,
                input.SourceAccountId,
                input.DestinationAccountId,
                input.Amount,
                cancellationToken
            );

            // 10. Retornar resultado
            return Result<TransferMoneyOutput>.Success(new TransferMoneyOutput(
                transaction.Id,
                sourceAccount.Balance,
                destinationAccount.Balance,
                transaction.CreatedAt
            ));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
