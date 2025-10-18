namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryItemAggregate;

/// <summary>
/// Represents a loan of an inventory item in the tool library system.
/// </summary>
public class Loan(LoanId id) : Entity<LoanId>(id)
{
    public void Test()
    {
        var t = Tool.Create(null , null ,new Manufacturer(1), ToolType.ConstructionTool ); 
    }    
}

