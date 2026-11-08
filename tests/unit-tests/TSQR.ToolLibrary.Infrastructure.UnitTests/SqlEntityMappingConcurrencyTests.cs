using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;

namespace TSQR.ToolLibrary.Infrastructure.UnitTests;

public sealed class SqlEntityMappingConcurrencyTests
{
    private static readonly ISqlEntityMapping[] Mappings =
    [
        new InventoryItemMapping(),
        new LoanMapping(),
        new MaintenanceRecordMapping(),
        new MemberMapping(),
        new PolicyMapping(),
        new ReservationMapping(),
        new ToolMapping(),
    ];

    [Fact]
    public void EveryUpdateStatementChecksAndReturnsRowVersion()
    {
        foreach (var mapping in Mappings)
        {
            Assert.True(
                mapping.UpdateSql.Contains(
                    "xmin::text::bigint = @RowVersion",
                    StringComparison.Ordinal),
                $"{mapping.TableName} update must include the row-version predicate.");

            Assert.True(
                mapping.UpdateSql.TrimEnd().EndsWith(
                    "RETURNING xmin::text::bigint",
                    StringComparison.Ordinal),
                $"{mapping.TableName} update must return the new row version.");
        }
    }

    [Fact]
    public void EveryDeleteStatementChecksRowVersion()
    {
        foreach (var mapping in Mappings)
        {
            Assert.True(
                mapping.DeleteSql.Contains(
                    "xmin::text::bigint = @RowVersion",
                    StringComparison.Ordinal),
                $"{mapping.TableName} delete must include the row-version predicate.");
        }
    }
}
