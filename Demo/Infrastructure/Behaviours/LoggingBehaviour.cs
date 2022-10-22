namespace Demo.Infrastructure.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _log;
    
    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> log)
    {
        _log = log;
    }
    
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _log.LogInformation("Handling {Name}", typeof(TRequest).FullName);
        var result = await next();
        _log.LogInformation("Handed {Name}", typeof(TRequest).FullName);
        return result;
    }
}