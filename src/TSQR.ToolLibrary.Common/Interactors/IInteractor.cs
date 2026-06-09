namespace TSQR.ToolLibrary.Common.Interactors;

public interface IInteractor<TCommand, TResult>
{
    Task<TResult> ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface IInteractor<TCommand>
{
    Task ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
}
