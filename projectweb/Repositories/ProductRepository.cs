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

        //  Filter op categorie en zoekterm
        public List<Product> GetByCategorieEnZoekterm(int categorieId, string? zoekterm)
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();

            string sql = @"
        SELECT Id, Naam, Prijs, CategorieId, ToonAddOnSuggestie
        FROM Product
        WHERE CategorieId = @CategorieId
        AND (@Zoekterm IS NULL OR Naam LIKE '%' + @Zoekterm + '%')
        ORDER BY Naam;
        ";

            return conn.Query<Product>(sql, new { CategorieId = categorieId, Zoekterm = zoekterm }).ToList();
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

        public Product? GetMetAddOns(int productId)
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();

            string sql = @"
SELECT 
    p.Id, p.Naam, p.Prijs, p.CategorieId,
    pac.Id, pac.ProductId, pac.AddOnCategorieId,
    aoc.Id, aoc.Naam, aoc.MeerdereKeuzes,
    ao.Id, ao.Naam, ao.AddOnCategorieId,
    pa.Id, pa.ProductId, pa.AddOnId, pa.Prijs
FROM Product p
LEFT JOIN ProductAddOnCategorie pac ON pac.ProductId = p.Id
LEFT JOIN AddOnCategorie aoc ON aoc.Id = pac.AddOnCategorieId
LEFT JOIN AddOn ao ON ao.AddOnCategorieId = aoc.Id
LEFT JOIN ProductAddOn pa ON pa.ProductId = p.Id AND pa.AddOnId = ao.Id
WHERE p.Id = @ProductId
";

            Product? product = null;

            conn.Query<Product, ProductAddOnCategorie, AddOnCategorie, AddOn, ProductAddOn, Product>(
                sql,
                (p, pac, aoc, ao, pa) =>
                {
                    product ??= p;
                    product.AddOnCategorieen ??= new();
                    product.AddOns ??= new();

                    if (pa != null && ao != null)
                    {
                        pa.AddOn = ao;
                        if (!product.AddOns.Any(x => x.AddOnId == pa.AddOnId))
                            product.AddOns.Add(pa);
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

                        if (!product.AddOnCategorieen.Any(c => c.AddOnCategorieId == aoc.Id))
                        {
                            pac.AddOnCategorieId = aoc.Id;
                            pac.ProductId = product.Id;
                            product.AddOnCategorieen.Add(pac);
                        }
                    }

                    return product;
                },
                new { ProductId = productId },
                splitOn: "Id,Id,Id,Id,Id"
            );

            return product;
        }

        public List<ProductAddOnView> GetAddOnsVoorProduct(int productId)
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();

            string sql = @"
        SELECT 
            p.Id AS ProductId,
            p.Naam AS ProductNaam,
            aoc.Id AS AddOnCategorieId,
            aoc.Naam AS AddOnCategorieNaam,
            aoc.MeerdereKeuzes AS MeerdereToegestaan,
            ao.Id AS AddOnId,
            ao.Naam AS AddOnNaam,
            pa.Prijs AS AddOnPrijs
        FROM Product p
        JOIN ProductAddOnCategorie pac ON pac.ProductId = p.Id
        JOIN AddOnCategorie aoc ON pac.AddOnCategorieId = aoc.Id
        JOIN AddOn ao ON ao.AddOnCategorieId = aoc.Id
        JOIN ProductAddOn pa ON pa.ProductId = p.Id AND pa.AddOnId = ao.Id
        WHERE p.Id = @ProductId
        ORDER BY aoc.Naam, ao.Naam;
    ";

            return conn.Query<ProductAddOnView>(sql, new { ProductId = productId }).ToList();
        }
        public List<Product> GetAllInclusiefGeldigeAddOns()
{
    using var conn = dbConnectionProvider.GetDatabaseConnection();

    var productDict = new Dictionary<int, Product>();

    string sql = @"
        SELECT 
            p.Id, p.Naam, p.Prijs, p.CategorieId, p.ToonAddOnSuggestie,
            pac.Id, pac.ProductId, pac.AddOnCategorieId,
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

    return productDict.Values.ToList();
}



    }
}
