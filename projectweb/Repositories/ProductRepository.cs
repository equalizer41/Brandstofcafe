using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace projectweb.Repositories
{
    public class ProductRepository
    {
        private readonly DbConnectionProvider dbConnectionProvider;

        public ProductRepository(DbConnectionProvider dbConnectionProvider)
        {
            this.dbConnectionProvider = dbConnectionProvider;
        }

        public List<Product> GetAll()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<Product>("SELECT * FROM Product").ToList();
        }

        public void Add(Product product)
        {
            const string sql = @"INSERT INTO Product (Naam, Prijs, CategorieId) 
                                 VALUES (@Naam, @Prijs, @CategorieId)";

            using var connection = dbConnectionProvider.GetDatabaseConnection();
            connection.Execute(sql, product);
        }
    }
}
