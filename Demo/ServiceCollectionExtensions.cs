﻿namespace Demo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDemo(this IServiceCollection services, IConfiguration configuration)
    {
        var tenantConnectionString = configuration["TenantConnectionString"];
        var adminConnectionString = configuration["AdminConnectionString"];

        var assembly = Assembly.GetExecutingAssembly();

        // api
        services.AddGrpc();
        
        // libraries
        services.AddMediatR(assembly);
        services.AddValidatorsFromAssembly(assembly);

        // infra
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IRepository, Repository>();
        services.AddScoped<ICarRepository, CarRepository>();
        services.AddScoped<IDbConnection>(c => new NpgsqlConnection(tenantConnectionString));

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