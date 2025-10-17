namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryItemAggregate;

/// <summary>
/// Represents the status of an inventory item.
/// </summary>
public enum ItemStatus
{
    Available = 0,
    Reserved,
    Loaned,
    UnderMaintenance,
    Lost 
}

