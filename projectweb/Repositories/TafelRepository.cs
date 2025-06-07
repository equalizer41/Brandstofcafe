using Dapper;

namespace projectweb.Repositories
{
    public class TafelRepository
    {

        private readonly DbConnectionProvider dbConnectionProvider;

        public TafelRepository(DbConnectionProvider dbConnectionProvider)
        {
            this.dbConnectionProvider = dbConnectionProvider;
        }
        public List<Tafel> GetAll()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<Tafel>("SELECT * FROM Tafel").ToList();
        }
    }
}
