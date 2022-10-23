using Demo.Api;
using Demo.Infrastructure.Behaviours;
using Demo.Infrastructure.Interceptors;
using Demo.Infrastructure.Tenancy;

namespace Demo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDemo(this IServiceCollection services, IConfiguration configuration)
    {
        var tenantConnectionString = configuration["TenantConnectionString"];
        var adminConnectionString = configuration["AdminConnectionString"];

        // libraries
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(assembly);
        services.AddValidatorsFromAssembly(assembly);

        // Setup grpc request pipeline
        services.AddGrpc()
            .AddServiceOptions<Service>(options =>
            {
                options.Interceptors.Add<ExceptionInterceptor>(); // Start trapping for exceptions
                options.Interceptors.Add<ValidationInterceptor>(); // validate the request
                options.Interceptors.Add<TenantContextInterceptor>(); // ge the tenant context
            });
        
        // Setup mediatr request pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>)); // start logging
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>)); // begin a transaction
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantContextBehaviour<,>)); // set tenant context in db
        
        // infra
        // Note that there is a single connection per request as this is a scoped dependency
        // This is important as this way the tenant context set by the TenantContextBehaviour
        // is the same one used by the repository
        services.AddScoped<IDbConnection>(c => new NpgsqlConnection(tenantConnectionString));
        services.AddScoped<ICarRepository, CarRepository>();

        // Tenant context
        // a single instance of tenant context is created per request
        // requests for ISet and IGet are both forwarded to the same instance 
        services.AddScoped<TenantContext>(); 
        services.AddScoped<IGetTenantContext>(c=>c.GetRequiredService<TenantContext>());
        services.AddScoped<ISetTenantContext>(c=>c.GetRequiredService<TenantContext>());

        // database migrations
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(adminConnectionString)
                .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());
        services
            .AddScoped<MigrationExecutor>();

        return services;
    }
}