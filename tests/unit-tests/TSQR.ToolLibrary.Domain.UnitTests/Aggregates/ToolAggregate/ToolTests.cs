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
        Manufacturer manufacturer = Manufacturer.Create(new ManufcaturerId(1),"manu");
        ToolType toolType = ToolType.HandTool;

        // Act
        Tool result = Tool.Create( model, description, manufacturer, toolType, metadata);

        // Assert
        Assert.Equal(model, result.Model);
        Assert.Equal(description, result.Description);
        Assert.Equal(manufacturer.Id, result.Manufacturer.Id);
        Assert.Equal(toolType, result.Type);
        Assert.Equal(metadata, result.Metadata);
    }

    [Fact]
    public void CreateFactoryMethod_ReturnsRehydratedEntity()
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

    [Fact]
    public void UpdateToolDetails_UpdatesDetails()
    {
        // Arrange
        ToolId id = new (1);
        Manufacturer manufacturer = Manufacturer.Create(new ManufcaturerId(1),"manu");
        ToolType toolType = ToolType.HandTool;
        Tool tool = Tool.Create(
                id, "old model", "old desc", Manufacturer.Create(new ManufcaturerId(2),"old manu"),
                ToolType.PowerTool, "old meta");
        // Act
        tool.UpdateToolDetails(model, description, manufacturer, toolType, metadata);

        // Assert
        Assert.Equal(model, tool.Model);
        Assert.Equal(description, tool.Description);
        Assert.Equal(manufacturer.Id, tool.Manufacturer.Id);
        Assert.Equal(toolType, tool.Type);
        Assert.Equal(metadata, tool.Metadata);
    }
}

