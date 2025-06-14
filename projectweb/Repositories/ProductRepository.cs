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

        //  Alle producten met categorie-info en add-ons
        public List<Product> GetAll()
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();

            var productDict = new Dictionary<int, Product>();

            string sql = @"
                SELECT 
                    p.Id, p.Naam, p.Prijs, p.CategorieId, p.ToonAddOnSuggestie,
                    pac.Id, pac.ProductId, pac.AddOnCategorieId, pac.Verplicht,
                    aoc.Id, aoc.Naam, aoc.MeerdereKeuzes,
                    ao.Id, ao.Naam, ao.AddOnCategorieId,
                    pa.Id, pa.ProductId, pa.AddOnId, pa.Prijs
                FROM Product p
                LEFT JOIN ProductAddOnCategorie pac ON pac.ProductId = p.Id
                LEFT JOIN AddOnCategorie aoc ON aoc.Id = pac.AddOnCategorieId
                LEFT JOIN AddOn ao ON ao.AddOnCategorieId = aoc.Id
                LEFT JOIN ProductAddOn pa ON pa.ProductId = p.Id AND pa.AddOnId = ao.Id
                ";

            var result = conn.Query<Product, ProductAddOnCategorie, AddOnCategorie, AddOn, ProductAddOn, Product>(
                sql,
                (product, pac, aoc, ao, pa) =>
                {
                    if (!productDict.TryGetValue(product.Id, out var existingProduct))
                    {
                        existingProduct = product;
                        existingProduct.AddOnCategorieen = new();
                        existingProduct.AddOns = new();
                        productDict[product.Id] = existingProduct;
                    }

                    if (pa != null && ao != null)
                    {
                        pa.AddOn = ao;
                        if (!existingProduct.AddOns.Any(x => x.AddOnId == pa.AddOnId))
                            existingProduct.AddOns.Add(pa);
                    }

                    if (pac != null && aoc != null)
                    {
                        pac.AddOnCategorie ??= new AddOnCategorie
                        {
                            Id = aoc.Id,
                            Naam = aoc.Naam,
                            MeerdereToegestaan = aoc.MeerdereToegestaan,
                            Opties = new List<AddOn>()
                        };

                        if (ao != null && !pac.AddOnCategorie.Opties.Any(o => o.Id == ao.Id))
                            pac.AddOnCategorie.Opties.Add(ao);

                        if (!existingProduct.AddOnCategorieen.Any(c => c.AddOnCategorieId == aoc.Id))
                        {
                            pac.AddOnCategorieId = aoc.Id;
                            pac.ProductId = product.Id;
                            existingProduct.AddOnCategorieen.Add(pac);
                        }
                    }

                    return existingProduct;
                },
                splitOn: "Id,Id,Id,Id,Id"
            );

            return result.Distinct().ToList();
        }

        //  Top 5 Meest bestelde producten
        public List<(string Naam, int TotaalAantal)> GetTopBesteld(int top = 5)
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();

            string sql = @"
            SELECT TOP (@Top) p.Naam, SUM(o.Aantal) AS TotaalAantal
            FROM Product p
            JOIN OrderRegel o ON o.ProductId = p.Id
            GROUP BY p.Naam
            ORDER BY TotaalAantal DESC;

            ";

            return conn.Query<(string Naam, int TotaalAantal)>(sql, new { Top = top }).ToList();
        }
        public List<Product> ZoekProducten(string zoekterm)
        {
            using (var connection = dbConnectionProvider.GetDatabaseConnection())
            {
                connection.Open();

                // SQL-query voor het zoeken naar producten op naam
                var sqlQuery = @"
                SELECT * 
                FROM Product
                WHERE Naam LIKE @zoekterm
                ORDER BY Naam";

                var producten = connection.Query<Product>(sqlQuery, new { zoekterm = "%" + zoekterm + "%" }).ToList();

                return producten;
            }




        }
    }
}
