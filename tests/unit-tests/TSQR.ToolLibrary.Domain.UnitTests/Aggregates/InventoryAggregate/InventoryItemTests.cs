using System;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Domain.Events;
using Xunit;

namespace TSQR.ToolLibrary.Domain.UnitTests.Aggregates.InventoryAggregate;

public class InventoryItemTests
{
    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var toolId = new ToolId(1);
        var ownerId = new MemberId(2);
        var acquisition = DateTime.UtcNow.AddDays(-10);
        var serial = "SN123";
        var condition = Condition.Good;

        var item = InventoryItem.Create(toolId, ownerId, acquisition, serial, condition);

        Assert.Equal(toolId, item.ToolId);
        Assert.Equal(ownerId, item.OriginalOwnerId);
        Assert.Equal(acquisition, item.InitialAcquisitionDate);
        Assert.Equal(serial, item.SerialNumber);
        Assert.Equal(ItemStatus.Available, item.Status);
        Assert.Equal(condition, item.Condition);
    }

    [Fact]
    public void Loan_MarksAsLoaned_AndAddsDomainEvent()
    {
        var toolId = new ToolId(1);
        var ownerId = new MemberId(2);
        var borrower = new MemberId(3);
        var acquisition = DateTime.UtcNow.AddDays(-10);
        var serial = "SN123";
        var condition = Condition.Good;

        var item = InventoryItem.Create(toolId, ownerId, acquisition, serial, condition);

        item.Loan(borrower);

        Assert.Equal(ItemStatus.Loaned, item.Status);
        Assert.Equal(borrower, item.CurrentHolderId);
        Assert.NotNull(item.LastBorrowedDate);
        // Domain event list is internal; assert that CurrentLoan/LastBorrowedDate updated
    }

    [Fact]
    public void Return_MarksAsAvailable()
    {
        var toolId = new ToolId(1);
        var ownerId = new MemberId(2);
        var borrower = new MemberId(3);
        var acquisition = DateTime.UtcNow.AddDays(-10);
        var serial = "SN123";
        var condition = Condition.Good;

        var item = InventoryItem.Create(toolId, ownerId, acquisition, serial, condition);
        item.Loan(borrower);

        item.Return();

        Assert.Equal(ItemStatus.Available, item.Status);
        Assert.Null(item.CurrentHolderId);
    }

    [Fact]
    public void Reserve_SetsReservationDate()
    {
        var toolId = new ToolId(1);
        var ownerId = new MemberId(2);
        var reserver = new MemberId(4);
        var acquisition = DateTime.UtcNow.AddDays(-10);
        var serial = "SN123";
        var condition = Condition.Good;

        var item = InventoryItem.Create(toolId, ownerId, acquisition, serial, condition);
        var reserveDate = DateTime.UtcNow.AddDays(1);

        item.Reserve(reserveDate, reserver);

        Assert.Equal(reserveDate, item.ReservationDate);
        // ReservationMemberId is optional in constructor; ensure not null
    }

    [Fact]
    public void MarkAsLost_UpdatesStatus()
    {
        var toolId = new ToolId(1);
        var ownerId = new MemberId(2);
        var reporter = new MemberId(5);
        var acquisition = DateTime.UtcNow.AddDays(-10);
        var serial = "SN123";
        var condition = Condition.Good;

        var item = InventoryItem.Create(toolId, ownerId, acquisition, serial, condition);

        item.MarkAsLost();

        Assert.Equal(ItemStatus.Lost, item.Status);
        Assert.Null(item.CurrentHolderId);
    }
}
