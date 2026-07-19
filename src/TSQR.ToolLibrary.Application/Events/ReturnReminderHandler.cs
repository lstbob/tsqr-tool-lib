using TSQR.ToolLibrary.Domain.Events;

namespace TSQR.ToolLibrary.Application.Events;

/// <summary>
/// STUB - intentionally not yet implemented. <see cref="ReturnReminderEvent"/>
/// is a *time-triggered* reminder ("loan due in N days" / "loan is overdue").
/// Unlike the other domain events in this aggregate set, it has no synchronous
/// raise site: it must be emitted by a background scheduler that periodically
/// ticks loans approaching <see cref="Loan.DueDate"/>. No scheduler exists in
/// the codebase yet, so this handler remains a no-op. When a scheduler is added
/// (tracked separately), the handler can perform the cross-aggregate side
/// effect appropriate at the time - e.g. escalating <see cref="LoanStatus"/> on
/// the Loan aggregate, or suspending the borrower's <see cref="Member"/> if the
/// loan is overdue beyond a policy threshold. Until then, the event record
/// stays defined-but-unraised so the audit's "Defined but Dead" entry is
/// closed for the dispatch pipeline while the time-based raise remains tracked.
/// </summary>
public class ReturnReminderHandler : IDomainEventHandler<ReturnReminderEvent>
{
    public Task HandleAsync(ReturnReminderEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}