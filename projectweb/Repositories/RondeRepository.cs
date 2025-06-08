using Dapper;

namespace projectweb.Repositories
{
    public class RondeRepository
    {
        private readonly DbConnectionProvider dbConnectionProvider;

        public RondeRepository(DbConnectionProvider dbConnectionProvider)
        {
            this.dbConnectionProvider = dbConnectionProvider;
        }

        public List<Ronde> GetAll()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();

            return connection.Query<Ronde>("SELECT * FROM Ronde").ToList();
        }

    }
}