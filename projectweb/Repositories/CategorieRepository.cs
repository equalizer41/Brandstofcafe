using Dapper;

namespace projectweb.Repositories
{
    public class CategorieRepository
    {
        private readonly DbConnectionProvider dbConnectionProvider;

        public CategorieRepository(DbConnectionProvider dbConnectionProvider)
        {
            this.dbConnectionProvider = dbConnectionProvider;
        }

        public List<Categorie> GetAll()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<Categorie>("SELECT * FROM Categorie").ToList();
        }

    }
}
