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

            var productDict = new Dictionary<int, Product>();  // Dictionary om producten op te slaan

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

            var resultaat = conn.Query<Product, ProductAddOnCategorie, AddOnCategorie, AddOn, ProductAddOn, Product>(
                sql,
                (product, pac, aoc, ao, pa) =>
                {
                    // Controleer of het product al bestaat in de dictionary
                    if (!productDict.TryGetValue(product.Id, out var bestaandProduct))
                    {
                        // Als het product nog niet bestaat, voeg het toe aan de dictionary
                        bestaandProduct = product;
                        bestaandProduct.AddOnCategorieen = new List<ProductAddOnCategorie>();  // Lijst voor add-on categorieën
                        bestaandProduct.AddOns = new List<ProductAddOn>();  // Lijst voor add-ons
                        productDict[product.Id] = bestaandProduct;
                    }

                    // Voeg de add-ons toe aan het product
                    if (pa != null && ao != null)
                    {
                        pa.AddOn = ao;  // Koppel de add-on aan het product

                        // Handmatige controle of de add-on al in de lijst zit
                        bool addOnBestaatAl = false;
                        foreach (var addOn in bestaandProduct.AddOns)
                        {
                            if (addOn.AddOnId == pa.AddOnId)
                            {
                                addOnBestaatAl = true;
                                break;
                            }
                        }

                        // Als de add-on nog niet bestaat, voeg deze dan toe
                        if (!addOnBestaatAl)
                        {
                            bestaandProduct.AddOns.Add(pa);
                        }
                    }

                    // Voeg de add-on categorieën toe aan het product
                    if (pac != null && aoc != null)
                    {
                        // Controleer of de add-on categorie al bestaat, zo niet maak dan een nieuwe aan
                        if (pac.AddOnCategorie == null)
                        {
                            pac.AddOnCategorie = new AddOnCategorie
                            {
                                Id = aoc.Id,
                                Naam = aoc.Naam,
                                MeerdereToegestaan = aoc.MeerdereToegestaan,
                                Opties = new List<AddOn>()  // Lijst van opties (add-ons)
                            };
                        }

                        // Handmatige controle of de add-on al bestaat in de lijst van opties
                        bool addOnCategorieBestaatAl = false;
                        foreach (var addOn in pac.AddOnCategorie.Opties)
                        {
                            if (addOn.Id == ao.Id)
                            {
                                addOnCategorieBestaatAl = true;
                                break;
                            }
                        }

                        // Als de add-on nog niet bestaat in de opties, voeg deze dan toe
                        if (!addOnCategorieBestaatAl && ao != null)
                        {
                            pac.AddOnCategorie.Opties.Add(ao);
                        }

                        // Controleer of de add-on categorie al aan het product is toegevoegd
                        bool addOnCategorieBestaatAlInProduct = false;
                        foreach (var categorie in bestaandProduct.AddOnCategorieen)
                        {
                            if (categorie.AddOnCategorieId == aoc.Id)
                            {
                                addOnCategorieBestaatAlInProduct = true;
                                break;
                            }
                        }

                        // Als de add-on categorie nog niet bestaat, voeg deze dan toe
                        if (!addOnCategorieBestaatAlInProduct)
                        {
                            pac.AddOnCategorieId = aoc.Id;
                            pac.ProductId = product.Id;
                            bestaandProduct.AddOnCategorieen.Add(pac);
                        }
                    }

                    return bestaandProduct;
                },
                splitOn: "Id,Id,Id,Id,Id"  // Geef aan welke velden worden gebruikt voor het splitsen van de resultaten
            );

            // Maak een lijst van unieke producten
            var uniekeProducten = new List<Product>();
            foreach (var product in productDict.Values)
            {
                uniekeProducten.Add(product);
            }

            return uniekeProducten;
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
