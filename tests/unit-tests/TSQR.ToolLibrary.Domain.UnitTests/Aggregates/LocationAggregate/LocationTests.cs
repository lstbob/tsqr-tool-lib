using TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;
using Xunit;

namespace TSQR.ToolLibrary.Domain.UnitTests.Aggregates.LocationAggregate;

public class LocationTests
{
    [Fact]
    public void CreateLocation_SetsName()
    {
        var location = Location.Create(new LocationId(default), "Main Branch");
        Assert.Equal("Main Branch", location.Name);
    }
}
