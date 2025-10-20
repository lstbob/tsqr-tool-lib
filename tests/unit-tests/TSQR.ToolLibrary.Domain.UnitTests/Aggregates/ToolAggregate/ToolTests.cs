namespace TSQR.ToolLibrary.Domain.UnitTests.Aggregates.ToolAggregate;

/// <summary>
/// Represents unit test class for the Tool domain entity.
/// </summary>
public class ToolTests
{
    private const string model = "model";
    private const string description = "description";
    private const string metadata = "metadata";

    [Fact]
    public void CreateFactoryMethod_ReturnsEntity()
    {
        // Arrange
        ToolId id = new ToolId(1);
        Manufacturer manufacturer = Manufacturer.Create(new ManufcaturerId(1),"manu");
        ToolType toolType = ToolType.HandTool;

        // Act
        Tool result = Tool.Create(id, model, description, manufacturer, toolType, metadata);

        // Assert
        Assert.Equal(id, result.Id);
        Assert.Equal(model, result.Model);
        Assert.Equal(description, result.Description);
        Assert.Equal(manufacturer.Id, result.Manufacturer.Id);
        Assert.Equal(toolType, result.Type);
        Assert.Equal(metadata, result.Metadata);
    }
}

