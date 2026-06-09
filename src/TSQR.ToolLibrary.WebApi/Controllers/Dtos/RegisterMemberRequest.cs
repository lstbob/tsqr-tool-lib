using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record RegisterMemberRequest(
    [Required][StringLength(100)] string FirstName,
    [StringLength(100)] string MiddleName,
    [Required][StringLength(100)] string LastName,
    [Range(0, 150)] int Age,
    [Required][StringLength(500)] string Address,
    [Required][StringLength(200)] string Email,
    [Required][StringLength(50)] string PhoneNumber,
    [Range(1, 99)] int Status,
    DateTime? MembershipStartDate = null,
    int? MembershipType = null);
