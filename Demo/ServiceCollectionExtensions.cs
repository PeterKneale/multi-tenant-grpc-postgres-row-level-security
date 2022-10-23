using Demo.Api;
using Demo.Application.Contracts;
using Demo.Infrastructure.Behaviours;
using Demo.Infrastructure.Configuration;
using Demo.Infrastructure.Interceptors;
using Demo.Infrastructure.Tenancy;

namespace Demo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDemo(this IServiceCollection services, IConfiguration configuration)
    {
        // libraries
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(assembly);
        services.AddValidatorsFromAssembly(assembly);

        // Setup grpc request pipeline
        services
            // Add common interceptors for both the admin and tenant api's
            .AddGrpc(options => {
                options.Interceptors.Add<ExceptionInterceptor>();// Start trapping for exceptions
                options.Interceptors.Add<ValidationInterceptor>();// validate the request
            })
            .AddServiceOptions<TenantApi>(options => {
                options.Interceptors.Add<TenantContextInterceptor>();// ge the tenant context
            });

        // Setup mediatr request pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));// start logging
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantTransactionBehaviour<,>));// begin a transaction
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantContextBehaviour<,>));// set tenant context in db

        // infra
        // Note that there is a single connection per request as this is a scoped dependency
        // This is important as this way the tenant context set by the TenantContextBehaviour
        // is the same one used by the repository
        services.AddScoped<IConnectionFactory,ConnectionFactory>();
        services.AddScoped<ICarRepository, CarRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();

        // Tenant context
        // a single instance of tenant context is created per request
        // requests for ISet and IGet are both forwarded to the same instance 
        services.AddScoped<TenantContext>();
        services.AddScoped<IGetTenantContext>(c => c.GetRequiredService<TenantContext>());
        services.AddScoped<ISetTenantContext>(c => c.GetRequiredService<TenantContext>());

        // database migrations
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(configuration.GetSystemConnectionString())
                .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());
        services
            .AddScoped<MigrationExecutor>();

        return services;
    }
}