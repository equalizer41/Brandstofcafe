using Dapper;

namespace projectweb.Repositories
{
    public class OrderRegelRepository
    {
        private readonly DbConnectionProvider dbConnectionProvider;

        public OrderRegelRepository(DbConnectionProvider dbConnectionProvider)
        {
            this.dbConnectionProvider = dbConnectionProvider;
        }

        public List<OrderRegel> GetAll()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();

            return connection.Query<OrderRegel>("SELECT * FROM OrderRegel").ToList();
        }

    }
}