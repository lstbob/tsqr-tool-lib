namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

// --- Members ---
public record MemberListItem(
    int Id,
    string FirstName,
    string MiddleName,
    string LastName,
    string FullName,
    int Age,
    string Email,
    string PhoneNumber,
    int Status,
    string StatusName,
    bool IsVerified,
    int? MembershipType,
    string? MembershipTypeName,
    DateTime? StartDate,
    DateTime? EndDate);

public record MemberDetail(
    int Id,
    string FirstName,
    string MiddleName,
    string LastName,
    string FullName,
    int Age,
    string Address,
    string Email,
    string PhoneNumber,
    int Status,
    string StatusName,
    bool IsVerified,
    int? VerifiedByAdminId,
    DateTime? VerificationDate,
    int? MembershipType,
    string? MembershipTypeName,
    DateTime? StartDate,
    DateTime? EndDate);

// --- Reservations ---
public record ReservationListItem(
    int Id,
    int ItemId,
    string ItemSerialNumber,
    string ToolModel,
    int MemberId,
    string MemberName,
    DateTime ReservationDate,
    DateTime ExpiryDate,
    int Status,
    string StatusName,
    bool IsConfirmed,
    int QueuePosition);

public record ReservationDetail(
    int Id,
    int ItemId,
    string ItemSerialNumber,
    string ToolModel,
    int MemberId,
    string MemberName,
    DateTime ReservationDate,
    DateTime ExpiryDate,
    int Status,
    string StatusName,
    bool IsConfirmed,
    int QueuePosition);

// --- Inventory ---
public record InventoryListItem(
    int Id,
    int ToolId,
    string ToolModel,
    string ToolTypeName,
    int? OriginalOwnerId,
    string? OriginalOwnerName,
    DateTime InitialAcquisitionDate,
    string SerialNumber,
    int Status,
    string StatusName,
    int Condition,
    string ConditionName,
    int? CurrentHolderId,
    string? CurrentHolderName,
    DateTime? LastBorrowedDate,
    int LoanCount,
    bool IsUnderRepair);

// --- Maintenance ---
public record MaintenanceListItem(
    int Id,
    int ItemId,
    string ItemSerialNumber,
    string ToolModel,
    int ReportedById,
    string ReportedByName,
    DateTime ReportedDate,
    string Description,
    int Status,
    string StatusName,
    int? CompletedById,
    string? CompletedByName,
    DateTime? CompletedDate,
    int? ResultingCondition,
    string? ResultingConditionName);