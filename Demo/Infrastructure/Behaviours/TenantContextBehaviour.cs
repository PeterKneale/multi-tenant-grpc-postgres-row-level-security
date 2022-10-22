using Dapper;
using Demo.Infrastructure.Tenancy;

namespace Demo.Infrastructure.Behaviours;

internal class TenantContextBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IDbConnection _connection;
    private readonly IGetTenantContext _context;
    private readonly ILogger<TenantContextBehaviour<TRequest, TResponse>> _log;
    
    public TenantContextBehaviour(IDbConnection connection, IGetTenantContext context, ILogger<TenantContextBehaviour<TRequest, TResponse>> log)
    {
        _connection = connection;
        _context = context;
        _log = log;
    }
    
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _log.LogInformation("Setting tenant context {Tenant}", _context.CurrentTenant);
        await SetConnectionContext();
        _log.LogInformation("Executing pipeline");
        return await next();
    }

    private async Task SetConnectionContext()
    {
        var tenant = _context.CurrentTenant;
        var sql = $"SET app.tenant = '{tenant}';";
        await _connection.ExecuteAsync(sql);
    }
}