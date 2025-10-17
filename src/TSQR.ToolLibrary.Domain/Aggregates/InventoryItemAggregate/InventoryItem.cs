using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryItemAggregate;

/// <summary>
/// Represents an inventory item in the tool library system.
/// </summary>
public class InventoryItem : Entity<InventoryItemId>
{
 
    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryItem"/> class.
    /// </summary>
    private InventoryItem(
            InventoryItemId id,
            MemberId originalOwnerId,
            DateTime initialAcquisitionDate,
            bool isAvailable = true,
            AmortizationRate amortizationRate,
            MemberId? currentHolderId = null,
            DateTime? lastBorrowedDate = null,
            DateTime? reservationDate = null,
            MemberId? reservationMember = null,
            ) : base(id)
    {

    }

    public MemberId OriginalOwnerId { get; }
    public DateTime InitialAcquisitionDate { get; }
    public bool IsAvailable { get;private set; }
    public AmortizationRate AmortizationRate { get; private set; }
    public MemberId? CurrentHolderId { get; private set; }
    public DateTime? LastBorrowedDate { get; private set; }
    public DateTime? ReservationDate { get; private set; }    
    public MemberId? ReservationMember {get; private set;}


}

