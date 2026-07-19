using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.Reservation;
using ToolAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Tool;
using PolicyAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Policy;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record ReserveToolCommand(
    InventoryItemId ItemId,
    MemberId MemberId,
    DateTime ReservationDate,
    int CommunityId = 1);

public class ReserveToolCommandHandler(
    IRepository<ReservationAgg, ReservationId> reservationRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IToolRepository toolRepository,
    IPolicyRepository policyRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<ReserveToolCommand, Result<ReservationId>>
{
    public async Task<Result<ReservationId>> ExecuteAsync(ReserveToolCommand command, CancellationToken cancellationToken)
    {
        // Load the InventoryItem so we can resolve the Tool, and from there the
        // active Policy. The (ToolType, null) lookup returns the global policy
        // for the tool type; per-location policies are an additive follow-up.
        // If no policy is configured, the handler rejects the reservation:
        // reserving without an enforced MaxLoanReservationDays window would
        // silently fall back to the legacy 14-day default - the orphaned-policy
        // pattern this command was refactored to close (issue #35).
        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(command.ItemId), "Inventory item not found.");

        var tool = await toolRepository.GetByIdAsync(item.ToolId, cancellationToken);
        if (tool is null)
            return new NotFoundError(nameof(item.ToolId), "Tool referenced by inventory item not found.");

        var policy = await policyRepository.GetByToolTypeAsync(tool.Type, locationId: null, cancellationToken);
        if (policy is null)
            return new DomainError(nameof(PolicyAgg), "No lending policy configured for this tool type.");

        // Policy-driven factory: Reservation.ExpiryDate is derived from
        // Policy.MaxLoanReservationDays instead of the legacy hardcoded 14.
        var reservationResult = ReservationAgg.Create(
            command.ItemId,
            command.MemberId,
            command.ReservationDate,
            policy.MaxLoanReservationDays,
            command.CommunityId);

        if (reservationResult.IsFailure)
            return reservationResult.Error;

        var reservation = reservationResult.Value;
        await reservationRepository.AddAsync(reservation, cancellationToken);
        await orchestrator.SaveEntitiesAsync(reservation, cancellationToken);

        return reservation.Id;
    }
}