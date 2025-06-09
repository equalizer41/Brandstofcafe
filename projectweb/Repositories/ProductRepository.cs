using Dapper;
using projectweb.Models;

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

        public List<ProductAddOn> GetAllProductAddOns()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();

            var sql = @"
                SELECT 
                    pa.Id, pa.ProductId, pa.AddOnId, pa.Prijs,
                    a.Id, a.Naam, a.AddOnCategorieId
                FROM ProductAddOn pa
                JOIN AddOn a ON pa.AddOnId = a.Id";

            var result = connection.Query<ProductAddOn, AddOn, ProductAddOn>(
                sql,
                (productAddOn, addOn) =>
                {
                    productAddOn.AddOn = addOn;
                    return productAddOn;
                },
                splitOn: "Id" 
            ).ToList();

            return result;
        }

        public List<ProductAddOnCategorie> GetAllProductAddOnCategorieen()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<ProductAddOnCategorie>("SELECT * FROM ProductAddOnCategorie").ToList();
        }

    }
}
