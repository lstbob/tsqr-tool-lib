namespace TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

///<summary>
///Represents a status of a member of the tool library
///</summary>
public enum MemberStatus
{
    NotSet = 0,
    Active,
    Suspended,
    Banned
}
