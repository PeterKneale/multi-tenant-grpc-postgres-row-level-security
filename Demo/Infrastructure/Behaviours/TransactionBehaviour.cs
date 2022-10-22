namespace Demo.Infrastructure.Behaviours;

internal class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IDbConnection _connection;
    private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _log;

    public TransactionBehaviour(IDbConnection connection, ILogger<TransactionBehaviour<TRequest, TResponse>> log)
    {
        _connection = connection;
        _log = log;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _log.LogInformation("Opening connection");
        _connection.Open();
        
        _log.LogInformation("Beginning transaction");
        var transaction = _connection.BeginTransaction();

        TResponse result;
        try
        {
            result = await next();

            _log.LogInformation("Committing transaction");
            transaction.Commit();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Rolling back transaction");
            transaction.Rollback();
            throw;
        }
        finally
        {
            _log.LogError("Closing connection");
            _connection.Close();
        }
        return result;
    }
}