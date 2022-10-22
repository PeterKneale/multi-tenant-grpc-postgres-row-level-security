using Demo.Infrastructure.Tenancy;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Demo.Infrastructure.Interceptors;

internal class TenantContextInterceptor : Interceptor
{
    private readonly ISetTenantContext _context;
    private readonly ILogger<TenantContextInterceptor> _log;

    public TenantContextInterceptor(ISetTenantContext context, ILogger<TenantContextInterceptor> log)
    {
        _context = context;
        _log = log;
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _log.LogInformation("Getting tenant context from grpc headers");

        var header = context.RequestHeaders.SingleOrDefault(x => x.Key == "tenant");
        if (header == null)
        {
            _log.LogError("No header found");
            throw new Exception("No header found");
        }

        _log.LogInformation("Found tenant header {Tenant}", header.Value);
        var tenant = header.Value;

        _context.SetCurrentTenant(tenant);

        return continuation(request, context);
    }
}