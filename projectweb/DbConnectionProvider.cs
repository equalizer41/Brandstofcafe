using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace projectweb
{
    public class DbConnectionProvider
    {
        private readonly IConfiguration configuration;

        public DbConnectionProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IDbConnection GetDatabaseConnection()
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            return new SqlConnection(connectionString);
        }
    }
}
