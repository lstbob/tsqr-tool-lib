namespace TSQR.ToolLibrary.Application.Contracts;

public interface IRequestHandler<TRequest, TResponse>
{
    TResponse Handle(TRequest request);
}

public interface IRequestHandlerAsync<TRequest, TResponse>
{
    TResponse HandleAsync(TRequest request, CancellationToken cancellationToken);
}
