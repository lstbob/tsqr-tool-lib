namespace TSQR.ToolLibrary.Application.Exceptions;

public class ApplicationExceptionHandler : IPipelineBehavior<TRequest, TResult>
    where TRequest is IRequest, TResult is IResult;
{
   
   // TODO: implement application exception handler that is not part of web pipeline
   // e.g. http handlers middlewares etc.
}

