namespace TSQR.ToolLibrary.Domain.Services;

public class FineService
{
    private const decimal DefaultLateFeePerDay = 1.00m;

    public decimal CalculateFine(Loan loan, DateTime returnDate)
    {
        ArgumentNullException.ThrowIfNull(loan);

        if (returnDate <= loan.DueDate)
            return 0m;

        var daysOverdue = (int)Math.Ceiling((returnDate - loan.DueDate).TotalDays);
        return daysOverdue * DefaultLateFeePerDay;
    }

    public decimal CalculateFine(Loan loan, DateTime returnDate, decimal lateFeePerDay)
    {
        ArgumentNullException.ThrowIfNull(loan);

        if (lateFeePerDay < 0)
            throw new ArgumentException("Late fee cannot be negative.", nameof(lateFeePerDay));

        if (returnDate <= loan.DueDate)
            return 0m;

        var daysOverdue = (int)Math.Ceiling((returnDate - loan.DueDate).TotalDays);
        return daysOverdue * lateFeePerDay;
    }
}
