namespace TSQR.ToolLibrary.Domain.Events;

public record ToolRegisteredEvent(ToolId ToolId, MemberId OwnerId, string Model, ToolType Type) : IDomainEvent;
