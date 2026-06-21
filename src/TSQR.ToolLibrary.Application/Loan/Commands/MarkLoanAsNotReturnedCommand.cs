using LoanAgg = TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate.Loan;

namespace TSQR.ToolLibrary.Application.Loan.Commands;

public record MarkLoanAsNotReturnedCommand(LoanId LoanId);

public class MarkLoanAsNotReturnedCommandHandler(
    IRepository<LoanAgg, LoanId> loanRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<MarkLoanAsNotReturnedCommand, Result>
{
    public async Task<Result> ExecuteAsync(MarkLoanAsNotReturnedCommand command, CancellationToken cancellationToken)
    {
        var loan = await loanRepository.GetByIdAsync(command.LoanId, cancellationToken);
        if (loan is null)
            return new NotFoundError(nameof(command.LoanId), "Loan not found.");

        var endResult = loan.EndLoan(DateTime.UtcNow);
        if (endResult.IsFailure)
            return endResult.Error;

        loanRepository.Update(loan);
        await orchestrator.SaveEntitiesAsync(loan, cancellationToken);

        return Result.Success();
    }
}
