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

        public Categorie? GetById(int id)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.QueryFirstOrDefault<Categorie>(
                "SELECT * FROM Categorie WHERE Id = @Id", new { Id = id });
        }

        public List<Categorie> GetRootCategorieen()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<Categorie>(
                "SELECT * FROM Categorie WHERE OuderCategorieId IS NULL").ToList();
        }

        public List<Categorie> GetSubCategorieen(int ouderCategorieId)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<Categorie>(
                "SELECT * FROM Categorie WHERE OuderCategorieId = @Id", new { Id = ouderCategorieId }).ToList();
        }
    }
}
