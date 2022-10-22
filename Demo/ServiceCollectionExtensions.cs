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

        var assembly = Assembly.GetExecutingAssembly();

        // api
        services.AddGrpc()
            .AddServiceOptions<Service>(options =>
            {
                options.Interceptors.Add<ExceptionInterceptor>(); // Start trapping for exceptions
                options.Interceptors.Add<ValidationInterceptor>(); // validate the request
                options.Interceptors.Add<TenantContextInterceptor>(); // ge the tenant context
            });
        
        // libraries
        services.AddMediatR(assembly);
        services.AddValidatorsFromAssembly(assembly);

        // infra
        services.AddScoped<ICarRepository, CarRepository>();
        services.AddScoped<IDbConnection>(c => new NpgsqlConnection(tenantConnectionString));
        
        // Tenant context
        // a single instance of tenant context is created per request
        // requests for ISet and IGet are both forwarded to the same instance 
        services.AddScoped<TenantContext>(); 
        services.AddScoped<IGetTenantContext>(c=>c.GetRequiredService<TenantContext>());
        services.AddScoped<ISetTenantContext>(c=>c.GetRequiredService<TenantContext>());

        // behaviours
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>)); // start logging
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>)); // begin a transaction
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantContextBehaviour<,>)); // set tenant context in db
        
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