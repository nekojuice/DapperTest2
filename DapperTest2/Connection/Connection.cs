using Microsoft.Data.SqlClient;
using System.Reflection;

namespace DapperTest2.Connection
{
    public static class Connection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            return services;
        }
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
            return services;
        }
    }


    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection();
    }
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly IConfiguration _configuration;
        public SqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("ApiConnectionString"));
        }
    }
}
