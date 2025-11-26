using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.ValueObjects;
using FamilyBudgeting.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FamilyBudgeting.API.Configuration
{
    public static class PostgresConfiguration
    {
        public static IServiceCollection AddPostgresWithEnums(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringName)
        {
            SqlMapper.AddTypeHandler<InvitationStatus>(new InvitationStatusTypeHandler());
            SqlMapper.AddTypeHandler<DestinationType>(new DestinationTypeHandler());

            string connectionString = configuration.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

            // Map PostgreSQL enum types
            dataSourceBuilder.MapEnum<InvitationStatus>("InvitationStatus");
            dataSourceBuilder.MapEnum<DestinationType>("DestinationType");

            var dataSource = dataSourceBuilder.Build();

            services.AddSingleton(dataSource);

            // Also register EF Core to use the same connection string
            services.AddDbContextFactory<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

            // Register Dapper SqlConnectionFactory if you're using it
            services.AddScoped<ISqlConnectionFactory>(provider => new SqlConnectionFactory(dataSource));

            return services;
        }
    }
}
