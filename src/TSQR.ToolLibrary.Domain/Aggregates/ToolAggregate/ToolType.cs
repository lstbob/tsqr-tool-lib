namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

public enum ToolType 
{
    /// <summary>
    /// Represents a hand tool.E.g., hammer, screwdriver.
    /// </summary>
    HandTool = 0,

    /// <summary>
    /// Represents a power tool. E.g., drill, saw.
    /// </summary>
    PowerTool,

    /// <summary>
    /// Represents a gardening tool. E.g., shovel, rake.
    /// </summary>
    GardeningTool,

    /// <summary>
    /// Represents a construction tool. E.g., level, trowel.
    /// </summary>
    ConstructionTool,

    /// <summary>
    /// Represents a specialty tool. E.g., pressure washer, welder, concrete mixer.
    /// </summary>
    SpecialtyTool,
    
    /// <summary>
    /// Represents other types of tools not categorized above.
    /// </summary>
    Other 
}

