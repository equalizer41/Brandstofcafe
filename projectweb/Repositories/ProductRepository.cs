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
            using var conn = dbConnectionProvider.GetDatabaseConnection();

            var productDict = new Dictionary<int, Product>();
            var addonCategorieDict = new Dictionary<int, AddOnCategorie>();

            string sql = @"
SELECT 
    p.Id, p.Naam, p.Prijs, p.CategorieId,

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
            existingProduct.AddOnCategorieen = new List<ProductAddOnCategorie>();
            existingProduct.AddOns = new List<ProductAddOn>(); // 👈 INIT
            productDict.Add(product.Id, existingProduct);
        }

        // 👇 Add de ProductAddOn inclusief prijs
        if (pa != null && !existingProduct.AddOns.Any(x => x.AddOnId == pa.AddOnId))
        {
            pa.AddOn = ao; // Link add-on zelf nog
            existingProduct.AddOns.Add(pa);
        }

        if (pac != null && aoc != null)
        {
            if (pac.AddOnCategorie == null)
            {
                pac.AddOnCategorie = aoc;
                pac.AddOnCategorie.Opties = new List<AddOn>();
            }

            if (ao != null && !pac.AddOnCategorie.Opties.Any(x => x.Id == ao.Id))
            {
                pac.AddOnCategorie.Opties.Add(ao);
            }

            //  Zorg dat AddOnCategorie ook wordt aangemaakt als pac null is
            if (ao != null && aoc != null)
            {
                // Check of deze categorie al is toegevoegd
                var bestaandePac = existingProduct.AddOnCategorieen
                    .FirstOrDefault(x => x.AddOnCategorieId == aoc.Id);

                if (bestaandePac == null)
                {
                    bestaandePac = new ProductAddOnCategorie
                    {
                        ProductId = existingProduct.Id,
                        AddOnCategorieId = aoc.Id,
                        AddOnCategorie = new AddOnCategorie
                        {
                            Id = aoc.Id,
                            Naam = aoc.Naam,
                            MeerdereToegestaan = aoc.MeerdereToegestaan,
                            Opties = new List<AddOn>()
                        }
                    };
                    existingProduct.AddOnCategorieen.Add(bestaandePac);
                }

                // Voeg de optie toe
                if (!bestaandePac.AddOnCategorie.Opties.Any(o => o.Id == ao.Id))
                    bestaandePac.AddOnCategorie.Opties.Add(ao);
            }

        }

        return existingProduct;
    },
    splitOn: "Id,Id,Id,Id,Id"
);


            return result.Distinct().ToList();
        }

    }
}
