namespace TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;

/// <summary>
/// Represents the status of a loan in the tool library system.
/// </summary>
public enum LoanStatus
{
    /// <summary>
    /// The loan status has not been set.
    /// </summary>
    NotSet = 0,

    /// <summary>
    /// The loan is currently active.
    /// </summary>
    Active,

    /// <summary>
    /// The loan has been returned.
    /// </summary>
    Returned,

    /// <summary>
    /// The loan is overdue.
    /// </summary>
    Overdue,

    /// <summary>
    /// The loan has been canceled.
    /// </summary>
    Canceled 
}

