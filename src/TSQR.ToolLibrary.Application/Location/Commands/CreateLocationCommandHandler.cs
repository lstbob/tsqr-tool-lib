namespace TSQR.ToolLibrary.Application.Location.Commands;

public class CreateLocationCommandHandler(IRepository<Location> repository)
    : IRequestHandler<CreateLocationCommand, IResult<Unit>>
{
    public async Task<CreateLocationResult> Handle(
        CreateLocationCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            Location location = Location.Create(request.Name, request.Country, request.Address);
            LocationId id = await repository.UnitOfWork.AddAsync(location);
            return CreateLocationResult(id, ResultType.Success);
        }
        catch (ArgumentNullException ex)
        {
            return new CreateLocationResult(default, ResultType.InvalidInput);
        }
        catch (ArgumentException ex)
        {
            return new CreateLocationResult(default, ResultType.InvalidInput);
        }
        catch (InvalidOperationException ex)
        {
            return new CreateLocationResult(default, ResultType.InvalidOperationRequested);
        }
        catch (Exception ex)
        {
            return new CreateLocationResult(default, ResultType.UnhandledError);
        }
    }
}

public record CreateLocationCommand(string Name, string Country, string Address)
    : IRequest<CreateLocationResult>;

public record CreateLocationResult(LocationId? Id, ResultType ResultType) : IResult;
