using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Represents a tool in the tool library system.
/// </summary>
public class Tool : Entity<ToolId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tool"/> class.
    /// </summary>
    private Tool(
        ToolId id,
        string model,
        string description,
        string manufacturer,
        string serialNumber,
        MemberId originalOwnerId,
        DateTime initialAcquisitionDate,
        AmortizationRate amortizationRate) : base(id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
        SerialNumber = serialNumber ?? throw new ArgumentNullException(nameof(serialNumber));
        OriginalOwnerId = originalOwnerId ?? throw new ArgumentNullException(nameof(originalOwnerId));
        InitialAcquisitionDate = initialAcquisitionDate == default ?
            throw new ArgumentNullException(nameof(initialAcquisitionDate)) : initialAcquisitionDate;
        OriginalOwnerId = originalOwnerId;
        IsAvailable = true;
        AmortizationRate = amortizationRate;
    } 

    public ToolId Id { get; }
    public string Model { get; }
    public string Description { get; }
    public string Manufacturer { get; }
    public string SerialNumber { get; }

    /// <summary>
    /// Factory method to create a new instance of the <see cref="Tool"/> class.
    /// </summary>
    public static Tool Create(
        string model,
        string description,
        string manufacturer,
        string serialNumber,
        MemberId originalOwnerId,
        DateTime initialAcquisitionDate,
        AmortizationRate amortizationRate)
    {
        return new Tool(
            new ToolId(default),
            model,
            description,
            manufacturer,
            serialNumber,
            originalOwnerId,
            initialAcquisitionDate,
            amortizationRate);
    }
    
    /// <summary>
    /// Factory method to rehydrate an existing instance of the <see cref="Tool"/> class.
    /// </summary>
    public static Tool Create(
        ToolId id,
        string model,
        string description,
        string manufacturer,
        string serialNumber,
        MemberId originalOwnerId,
        DateTime initialAcquisitionDate,
        bool isAvailable,
        MemberId currentHolder,
        DateTime? lastBorrowedDate,
        DateTime? nextBorrowedDate,
        AmortizationRate amortizationRate)
    {
        var tool = new Tool(
            id,
            model,
            description,
            manufacturer,
            serialNumber,
            originalOwnerId,
            initialAcquisitionDate);
        
        tool.CurrentHolderId = currentHolder;
        tool.IsAvailable = isAvailable;
        tool.LastBorrowedDate = lastBorrowedDate;
        tool.ReservationDate = nextBorrowedDate;

        return tool;
    }

    public Tool Register()
    {
    }

    /// <summary>
    /// Marks the tool as lost.
    /// </summary>
    public void MarkAsLost()
    {
        if (IsAvailable)
            throw new InvalidOperationException("Tool is already available.");

        CurrentHolderId = null;
        IsAvailable = false;
    }

    /// <summary>
    /// Returns the tool to the library.
    /// </summary>
    public void Return()
    {
        if (IsAvailable)
            throw new InvalidOperationException("Tool is already available.");

        CurrentHolderId = null;
        IsAvailable = true;
    }

    /// <summary>
    /// Borrows the tool to a member.
    /// </summary>
    public void Borrow(MemberId borrower, DateTime borrowDate)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Tool is not available for borrowing.");

        ArgumentNullException.ThrowIfNull(borrower);

        if (borrowDate == default)
            throw new ArgumentNullException(nameof(borrowDate));

        if (ReservationDate is not null && ReservationDate != borrowDate)
            throw new ArgumentException("Tool");

        CurrentHolderId = borrower;
        IsAvailable = false;
        LastBorrowedDate = borrowDate;
        ReservationDate = null;
    }

    /// <summary>
    /// Reserves the tool for a member on a specific date.
    /// </summary>  
    public void Reserve(DateTime reserveDate, MemberId borrower)
    {
        if(ReservationDate is not null)
            throw new InvalidOperationException("Tool is already reserved.");

        ArgumentNullException.ThrowIfNull(borrower);

        if (reserveDate == default || reserveDate <= DateTime.UtcNow)
            throw new ArgumentNullException(nameof(reserveDate));

        // TODO: Change when loan policy is introduced
        if(reserveDate > DateTime.UtcNow.AddYears(1)) 
            throw new InvalidOperationException("Tool cannot be reserved more than a year in advance.");

        ReservationDate = reserveDate;
    }
}

