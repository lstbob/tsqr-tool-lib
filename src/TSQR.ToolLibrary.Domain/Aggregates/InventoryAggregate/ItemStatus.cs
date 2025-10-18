namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;

/// <summary>
/// Represents the status of an inventory item.
/// </summary>
public enum ItemStatus
{
    NotSet = 0,
    Available ,
    Reserved,
    Loaned,
    UnderMaintenance,
    Lost 
}

