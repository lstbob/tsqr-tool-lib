namespace TSQR.ToolLibrary.Domain.Entities;

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
        string name,
        string description,
        string manufacturer,
        string serialNumber,
        Member originalOwner,
        DateTime initialAcquisitionDate)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
        SerialNumber = serialNumber ?? throw new ArgumentNullException(nameof(serialNumber));
        OriginalOwner = originalOwner ?? throw new ArgumentNullException(nameof(originalOwner));
        InitialAcquisitionDate = initialAcquisitionDate == default ?
            throw new ArgumentNullException(nameof(initialAcquisitionDate)) : initialAcquisitionDate;
        OriginalOwner = originalOwner;
        IsAvailable = true;
    }


    public string Name { get; }
    public string Description { get; }
    public string Manufacturer { get; }
    public string SerialNumber { get; }
    public Member OriginalOwner { get; }
    public bool IsAvailable { get; }
    public Member CurrentHolder { get; private set; }
    public DateTime? LastBorrowedDate { get; private set; }
    public DateTime? ReservationDate { get; private set; }    
    public Member? ReservationMember {get; private set;}
    public DateTime InitialAcquisitionDate { get; }

    /// <summary>
    /// Factory method to create a new instance of the <see cref="Tool"/> class.
    /// </summary>
    public static Tool Create(
        ToolId id,
        string name,
        string description,
        string manufacturer,
        string serialNumber,
        Member originalOwner,
        DateTime initialAcquisitionDate)
    {
        return new Tool(
            id,
            name,
            description,
            manufacturer,
            serialNumber,
            originalOwner,
            initialAcquisitionDate);
    }
    
    /// <summary>
    /// Factory method to rehydrate an existing instance of the <see cref="Tool"/> class.
    /// </summary>
    public static Tool Create(
        ToolId id,
        string name,
        string description,
        string manufacturer,
        string serialNumber,
        Member originalOwner,
        DateTime initialAcquisitionDate,
        bool isAvailable,
        Member currentHolder,
        DateTime? lastBorrowedDate,
        DateTime? nextBorrowedDate)
    {
        var tool = new (
            id,
            name,
            description,
            manufacturer,
            serialNumber,
            originalOwner,
            initialAcquisitionDate);
        
        tool.CurrentHolder = currentHolder;
        tool.IsAvailable = isAvailable;
        tool.LastBorrowedDate = lastBorrowedDate;
        tool.ReservationDate = nextBorrowedDate;

        return tool;
    }

    /// <summary>
    /// Marks the tool as lost.
    /// </summary>
    public void MarkAsLost()
    {
        if (IsAvailable)
            throw new InvalidOperationException("Tool is already available.");

        CurrentHolder = null;
        IsAvailable = false;
    }

    /// <summary>
    /// Returns the tool to the library.
    /// </summary>
    public void Return()
    {
        if (IsAvailable)
            throw new InvalidOperationException("Tool is already available.");

        CurrentHolder = null;
        IsAvailable = true;
    }

    /// <summary>
    /// Borrows the tool to a member.
    /// </summary>
    public void Borrow(Member borrower, DateTime borrowDate)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Tool is not available for borrowing.");

        if (borrower == null)
            throw new ArgumentNullException(nameof(borrower));

        if (borrowDate == default)
            throw new ArgumentNullException(nameof(borrowDate));

        if (ReservationDate is not null && ReservationDate != borrowDate)
            throw new ArgumentException("Tool");

        CurrentHolder = borrower;
        IsAvailable = false;
        LastBorrowedDate = borrowDate;
        ReservationDate = null;
    }

    /// <summary>
    /// Reserves the tool for a member on a specific date.
    /// </summary>  
    public void Reserve(DateTime reserveDate, Member borrower)
    {
        if(ReservationDate is not null)
            throw new InvalidOperationException("Tool is already reserved.");

        if (borrower == null)
            throw new ArgumentNullException(nameof(borrower));

        if (reserveDate == default || reserveDate <= DateTime.UtcNow)
            throw new ArgumentNullException(nameof(reserveDate));

        if(reserveDate > DateTime.UtcNow.AddYears(1)) 
            throw new InvalidOperationException("Tool cannot be reserved more than a year in advance.");

        ReservationDate = reserveDate;
    }
}

