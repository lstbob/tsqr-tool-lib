namespace TSQR.ToolLibrary.Domain.Events;

public record InventoryItemRequiredEvent(
    ToolId ToolId,
    MemberId OwnerId,
    string SerialNumber,
    Condition InitialCondition,
    int CommunityId) : IDomainEvent;
