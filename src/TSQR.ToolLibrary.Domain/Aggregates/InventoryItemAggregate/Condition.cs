namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryItemAggregate;

/// <summary>
/// Represents the condition of an inventory item. 
/// </summary>
public enum Condition 
{
    New = 0,
    Good,
    Fair,
    Repaired,
    Poor
}
