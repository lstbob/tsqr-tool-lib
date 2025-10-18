namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryItemAggregate;

/// <summary>
/// Represents the condition of an inventory item. 
/// </summary>
public enum Condition 
{
    NotSet = 0,
    New,
    Good,
    Fair,
    Repaired,
    Poor
}
