using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using Xunit;

namespace TSQR.ToolLibrary.Domain.UnitTests.Aggregates.ToolAggregate;

public class ManufacturerTests
{
    [Fact]
    public void CreateManufacturer_SetsName()
    {
        var m = Manufacturer.Create("Acme Tools");
        Assert.Equal("Acme Tools", m.Name);
    }
}
