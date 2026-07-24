namespace TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

public class Identification : ValueObject
{
    private Identification(IdentificationType type, string reference)
    {
        Type = type;
        Reference = reference;
    }

    public IdentificationType Type { get; }
    public string Reference { get; }

    public static Result<Identification> Create(IdentificationType type, string reference)
    {
        var typeResult = type.ValidateDefined(nameof(type));
        if (typeResult.IsFailure)
            return typeResult.Error;

        var notDefaultResult = type.ValidateNotDefault(nameof(type));
        if (notDefaultResult.IsFailure)
            return notDefaultResult.Error;

        var referenceResult = reference.Validate(nameof(reference));
        if (referenceResult.IsFailure)
            return referenceResult.Error;

        return new Identification(type, referenceResult.Value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Reference;
    }
}
