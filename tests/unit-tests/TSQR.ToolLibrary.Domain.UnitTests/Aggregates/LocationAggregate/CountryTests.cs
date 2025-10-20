using TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;
using Xunit;

namespace TSQR.ToolLibrary.Domain.UnitTests.Aggregates.LocationAggregate;

public class CountryTests
{
    [Fact]
    public void CreateCountry_SetsName()
    {
        var country = Country.Create("Canada");
        Assert.Equal("Canada", country.Name);
    }
}
