using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class ManufacturerValueObjectTests
{
    [Fact]
    public void Manufacturers_WithSameValues_AreEqual()
    {
        var left = Manufacturer.Create(new ManufacturerId(1), "Acme", communityId: 2);
        var right = Manufacturer.Create(new ManufacturerId(1), "Acme", communityId: 2);

        Assert.Equal(left, right);
        Assert.True(left == right);
    }

    [Fact]
    public void Manufacturers_WithDifferentValues_AreNotEqual()
    {
        var left = Manufacturer.Create(new ManufacturerId(1), "Acme", communityId: 2);
        var right = Manufacturer.Create(new ManufacturerId(2), "Acme", communityId: 2);

        Assert.NotEqual(left, right);
        Assert.True(left != right);
    }
}
