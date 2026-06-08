namespace TSQR.ToolLibrary.Domain.Services;

public class FineService
{
    private const decimal DefaultLateFeePerDay = 1.00m;

    public Result<decimal> CalculateFine(Loan loan, DateTime returnDate)
    {
        if (loan is null)
            return new ValidationError(nameof(loan), "Loan is required.");

        if (returnDate <= loan.DueDate)
            return 0m;

        var daysOverdue = (int)Math.Ceiling((returnDate - loan.DueDate).TotalDays);
        return daysOverdue * DefaultLateFeePerDay;
    }

    public Result<decimal> CalculateFine(Loan loan, DateTime returnDate, decimal lateFeePerDay)
    {
        if (loan is null)
            return new ValidationError(nameof(loan), "Loan is required.");

        if (lateFeePerDay < 0)
            return new ValidationError(nameof(lateFeePerDay), "Late fee cannot be negative.");

        if (returnDate <= loan.DueDate)
            return 0m;

        var daysOverdue = (int)Math.Ceiling((returnDate - loan.DueDate).TotalDays);
        return daysOverdue * lateFeePerDay;
    }
}
