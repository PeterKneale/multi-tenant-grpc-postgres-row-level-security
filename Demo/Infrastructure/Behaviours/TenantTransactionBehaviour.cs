using Demo.Application.Contracts;

namespace Demo.Infrastructure.Behaviours;

internal class TenantTransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IRequireTenantContext
{
    private readonly IDbConnection _connection;
    private readonly ILogger<TenantTransactionBehaviour<TRequest, TResponse>> _log;

    public TenantTransactionBehaviour(IConnectionFactory factory, ILogger<TenantTransactionBehaviour<TRequest, TResponse>> log)
    {
        _connection = factory.GetConnection();
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
            transaction.Dispose();
            _connection.Close();
        }
        return result;
    }
}