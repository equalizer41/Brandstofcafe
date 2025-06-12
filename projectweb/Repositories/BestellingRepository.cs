using Dapper;
using projectweb.Components;
using System;


namespace projectweb.Repositories
{
    public class BestellingRepository
    {
        private readonly DbConnectionProvider dbConnectionProvider;

        public BestellingRepository(DbConnectionProvider dbConnectionProvider)
        {
            this.dbConnectionProvider = dbConnectionProvider;
        }
        public List<Bestelling> GetAll()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<Bestelling>("SELECT * FROM Bestelling").ToList();
        }
        public Bestelling? GetById(int id)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.QueryFirstOrDefault<Bestelling>(
                "SELECT * FROM Bestelling WHERE Id = @Id", new { Id = id });
        }

        public List<Bestelling> OpenBestellingBijTafelID(int tafelId)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();

            var sql = @"
SELECT 
    b.Id,
    b.TafelId,
    b.IsBetaald,

    r.Id,
    r.RondNr,
    r.Tijdstip,
    r.Status,
    r.BestellingId,

    o.Id,
    o.Aantal,
    o.AantalBetaald,
    o.ProductId,
    o.RondeId,
    
    p.Id,
    p.Naam,
    p.Prijs,


    oa.Id,
    oa.OrderRegelId,
    oa.ProductAddOnId,

    pa.Id,
    pa.ProductId,
    pa.AddOnId,
    pa.Prijs,

    a.Id,
    a.Naam,
    a.AddOnCategorieId

FROM Bestelling b
JOIN Ronde r             ON r.BestellingId = b.Id
JOIN OrderRegel o        ON o.RondeId = r.Id
JOIN Product p           ON p.Id = o.ProductId  
LEFT JOIN OrderRegelAddOn oa ON oa.OrderRegelId = o.Id
LEFT JOIN ProductAddOn pa    ON pa.Id = oa.ProductAddOnId
LEFT JOIN AddOn a           ON a.Id = pa.AddOnId

WHERE b.TafelId = @TafelId
  AND b.Id = (
      SELECT MAX(Id)
      FROM Bestelling
      WHERE TafelId = @TafelId
  )
ORDER BY b.Id, r.Id, o.Id, oa.Id;
";

            var bestellingenCache = new Dictionary<int, Bestelling>();

            var result = connection.Query<Bestelling, Ronde, OrderRegel, Product, OrderRegelAddOn, ProductAddOn, AddOn, Bestelling>(
                sql,
                (bestelling, ronde, orderRegel, Product, orderRegelAddOn, productAddOn, addOn) =>
                {
                    if (!bestellingenCache.TryGetValue(bestelling.Id, out var bestaandeBestelling))
                    {
                        bestaandeBestelling = bestelling;
                        bestellingenCache.Add(bestelling.Id, bestaandeBestelling);
                    }

                    var bestaandeRonde = bestaandeBestelling.Rondes
                        .FirstOrDefault(r => r.Id == ronde.Id);
                    if (bestaandeRonde == null)
                    {
                        bestaandeRonde = ronde;
                        bestaandeBestelling.Rondes.Add(bestaandeRonde);
                    }

                    var bestaandeOrderRegel = bestaandeRonde.OrderRegels
                        .FirstOrDefault(o => o.Id == orderRegel.Id);
                    if (bestaandeOrderRegel == null)
                    {
                        bestaandeOrderRegel = orderRegel;
                        bestaandeOrderRegel.Product = Product;

                        bestaandeRonde.OrderRegels.Add(bestaandeOrderRegel);
                    }

                    if (orderRegelAddOn != null && orderRegelAddOn.Id != 0)
                    {
                        if (productAddOn != null)
                            orderRegelAddOn.ProductAddOn = productAddOn;

                        if (addOn != null && productAddOn != null)
                            productAddOn.AddOn = addOn;

                        bestaandeOrderRegel.AddOns.Add(orderRegelAddOn);
                    }

                    return bestaandeBestelling;
                },
                param: new { TafelId = tafelId },
                splitOn: "Id,Id,Id,Id,Id"
            );

            return result.Distinct().ToList();
        }

        public List<Bestelling> OpenAlleBestellingBijTafelID(int tafelId)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();

            var sql = @"
SELECT 
    b.Id,
    b.TafelId,
    b.IsBetaald,

    r.Id,
    r.RondNr,
    r.Tijdstip,
    r.Status,
    r.BestellingId,

    o.Id,
    o.Aantal,
    o.AantalBetaald,
    o.ProductId,
    o.RondeId,
    
    p.Id,
    p.Naam,
    p.Prijs,


    oa.Id,
    oa.OrderRegelId,
    oa.ProductAddOnId,

    pa.Id,
    pa.ProductId,
    pa.AddOnId,
    pa.Prijs,

    a.Id,
    a.Naam,
    a.AddOnCategorieId

FROM Bestelling b
JOIN Ronde r             ON r.BestellingId = b.Id
JOIN OrderRegel o        ON o.RondeId = r.Id
JOIN Product p           ON p.Id = o.ProductId  
LEFT JOIN OrderRegelAddOn oa ON oa.OrderRegelId = o.Id
LEFT JOIN ProductAddOn pa    ON pa.Id = oa.ProductAddOnId
LEFT JOIN AddOn a           ON a.Id = pa.AddOnId

WHERE b.TafelId = @TafelId

ORDER BY b.Id, r.Id, o.Id, oa.Id;
";

            var bestellingenCache = new Dictionary<int, Bestelling>();

            var result = connection.Query<Bestelling, Ronde, OrderRegel, Product, OrderRegelAddOn, ProductAddOn, AddOn, Bestelling>(
                sql,
                (bestelling, ronde, orderRegel, Product, orderRegelAddOn, productAddOn, addOn) =>
                {
                    if (!bestellingenCache.TryGetValue(bestelling.Id, out var bestaandeBestelling))
                    {
                        bestaandeBestelling = bestelling;
                        bestellingenCache.Add(bestelling.Id, bestaandeBestelling);
                    }

                    var bestaandeRonde = bestaandeBestelling.Rondes
                        .FirstOrDefault(r => r.Id == ronde.Id);
                    if (bestaandeRonde == null)
                    {
                        bestaandeRonde = ronde;
                        bestaandeBestelling.Rondes.Add(bestaandeRonde);
                    }

                    var bestaandeOrderRegel = bestaandeRonde.OrderRegels
                        .FirstOrDefault(o => o.Id == orderRegel.Id);
                    if (bestaandeOrderRegel == null)
                    {
                        bestaandeOrderRegel = orderRegel;
                        bestaandeOrderRegel.Product = Product;

                        bestaandeRonde.OrderRegels.Add(bestaandeOrderRegel);
                    }

                    if (orderRegelAddOn != null && orderRegelAddOn.Id != 0)
                    {
                        if (productAddOn != null)
                            orderRegelAddOn.ProductAddOn = productAddOn;

                        if (addOn != null && productAddOn != null)
                            productAddOn.AddOn = addOn;

                        bestaandeOrderRegel.AddOns.Add(orderRegelAddOn);
                    }

                    return bestaandeBestelling;
                },
                param: new { TafelId = tafelId },
                splitOn: "Id,Id,Id,Id,Id"
            );

            return result.Distinct().ToList();
        }

        public async Task<Ronde?> HerhaalRonde(int rondeId)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            connection.Open();


            var sql = @"
SELECT 
    r.Id, r.RondNr, r.Tijdstip, r.Status, r.BestellingId,

    o.Id, o.Aantal, o.AantalBetaald, o.ProductId, o.RondeId,

    p.Id, p.Naam, p.Prijs,

    oa.Id, oa.OrderRegelId, oa.ProductAddOnId,

    pa.Id, pa.ProductId, pa.AddOnId, pa.Prijs,

    a.Id, a.Naam, a.AddOnCategorieId

FROM Ronde r
JOIN OrderRegel o        ON o.RondeId = r.Id
JOIN Product p           ON p.Id = o.ProductId
LEFT JOIN OrderRegelAddOn oa ON oa.OrderRegelId = o.Id
LEFT JOIN ProductAddOn pa    ON pa.Id = oa.ProductAddOnId
LEFT JOIN AddOn a           ON a.Id = pa.AddOnId

WHERE r.Id = @RondeId
ORDER BY o.Id, oa.Id;
";

            var rondeCache = new Dictionary<int, Ronde>();

            // ❗️ Eerst uitvoeren om cache te vullen
            await connection.QueryAsync<Ronde, OrderRegel, Product, OrderRegelAddOn, ProductAddOn, AddOn, Ronde>(
                sql,
                (ronde, orderRegel, product, orderRegelAddOn, productAddOn, addOn) =>
                {
                    if (!rondeCache.TryGetValue(ronde.Id, out var bestaandeRonde))
                    {
                        bestaandeRonde = ronde;
                        bestaandeRonde.OrderRegels = new List<OrderRegel>();
                        rondeCache.Add(ronde.Id, bestaandeRonde);
                    }

                    var bestaandeRegel = bestaandeRonde.OrderRegels
                        .FirstOrDefault(r => r.Id == orderRegel.Id);
                    if (bestaandeRegel == null)
                    {
                        bestaandeRegel = orderRegel;
                        bestaandeRegel.Product = product;
                        bestaandeRegel.AddOns = new List<OrderRegelAddOn>();
                        bestaandeRonde.OrderRegels.Add(bestaandeRegel);
                    }

                    if (orderRegelAddOn != null && orderRegelAddOn.Id != 0)
                    {
                        if (productAddOn != null)
                            orderRegelAddOn.ProductAddOn = productAddOn;
                        if (productAddOn?.AddOn == null && addOn != null)
                            productAddOn.AddOn = addOn;

                        bestaandeRegel.AddOns.Add(orderRegelAddOn);
                    }

                    return bestaandeRonde;
                },
                param: new { RondeId = rondeId },
                splitOn: "Id,Id,Id,Id,Id"
            );

            // Haal nu de volledig opgebouwde ronde eruit
            var origineleRonde = rondeCache.Values.FirstOrDefault();
            if (origineleRonde == null)
                return null;

            // STAP 2: Maak nieuwe ronde op basis van origineel
            var nieuweRonde = new Ronde
            {
                RondNr = origineleRonde.RondNr + 1,
                Tijdstip = DateTime.Now,
                BestellingId = origineleRonde.BestellingId,
                OrderRegels = origineleRonde.OrderRegels.Select(r => new OrderRegel
                {
                    Aantal = r.Aantal,
                    AantalBetaald = 0,
                    ProductId = r.ProductId,
                    AddOns = r.AddOns.Select(a => new OrderRegelAddOn
                    {
                        ProductAddOnId = a.ProductAddOnId
                    }).ToList()
                }).ToList()
            };

            // STAP 3: Sla nieuwe ronde + regels + addons op in DB
            using var transaction = connection.BeginTransaction();

            // Insert ronde
            var rondeInsertSql = @"
                    INSERT INTO Ronde (RondNr, Tijdstip, Status, BestellingId)
                    VALUES (@RondNr, @Tijdstip, @Status, @BestellingId);
                    SELECT SCOPE_IDENTITY();
                ";
            nieuweRonde.Id = await connection.ExecuteScalarAsync<int>(rondeInsertSql, nieuweRonde, transaction);

            foreach (var regel in nieuweRonde.OrderRegels)
            {
                regel.RondeId = nieuweRonde.Id;

                var regelInsertSql = @"
            INSERT INTO OrderRegel (Aantal, AantalBetaald, ProductId, RondeId)
            VALUES (@Aantal, @AantalBetaald, @ProductId, @RondeId);
            SELECT SCOPE_IDENTITY();
        ";
                regel.Id = await connection.ExecuteScalarAsync<int>(regelInsertSql, regel, transaction);

                foreach (var addon in regel.AddOns)
                {
                    addon.OrderRegelId = regel.Id;
                    await connection.ExecuteAsync(@"
                INSERT INTO OrderRegelAddOn (OrderRegelId, ProductAddOnId)
                VALUES (@OrderRegelId, @ProductAddOnId);", addon, transaction);
                }
            }

            transaction.Commit();

            return nieuweRonde;
        }

        public List<RekeningItem> HaalRekeningItemsVoorTafel(int tafelId)
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();

                        var sql = @"
            SELECT 
                o.Id             AS OrderRegelId,
                p.Naam           AS ProductNaam,
                o.Aantal,
                o.AantalBetaald,
                p.Prijs          AS PrijsPerStuk
            FROM Bestelling b
            JOIN Ronde r ON r.BestellingId = b.Id
            JOIN OrderRegel o ON o.RondeId = r.Id
            JOIN Product p ON p.Id = o.ProductId
            WHERE b.TafelId = @TafelId
            ORDER BY r.RondNr, o.Id;
            ";

            return conn.Query<RekeningItem>(sql, new { TafelId = tafelId }).ToList();
        }

        public List<RekeningItem> HaalRekeningItemsStructuur(int tafelId)
        {
            var bestellingen = OpenAlleBestellingBijTafelID(tafelId);
            var result = new List<RekeningItem>();

            foreach (var bestelling in bestellingen)
            {
                foreach (var ronde in bestelling.Rondes)
                {
                    foreach (var regel in ronde.OrderRegels)
                    {
                        // Hoofdproduct
                        result.Add(new RekeningItem
                        {
                            BestellingId = bestelling.Id,
                            RondeNr = ronde.RondNr,
                            OrderRegelId = regel.Id,
                            ProductNaam = regel.Product?.Naam ?? "Onbekend",
                            Aantal = regel.Aantal,
                            AantalBetaald = regel.AantalBetaald,
                            PrijsPerStuk = regel.Product?.Prijs ?? 0,
                            IsAddOn = false
                        });

                        // AddOns
                        foreach (var addon in regel.AddOns)
                        {
                            result.Add(new RekeningItem
                            {
                                BestellingId = bestelling.Id,
                                RondeNr = ronde.RondNr,
                                OrderRegelId = regel.Id,
                                ProductNaam = addon.ProductAddOn?.AddOn?.Naam ?? "Onbekend",
                                Aantal = regel.Aantal,
                                AantalBetaald = regel.AantalBetaald,
                                PrijsPerStuk = addon.ProductAddOn?.Prijs ?? 0,
                                IsAddOn = true,
                                HoofdregelId = regel.Id
                            });
                        }
                    }
                }
            }

            return result;
        }

        public async Task BetaalGeselecteerdeAsync(IEnumerable<RekeningItem> items)
        {
            const string sql = @"
        UPDATE OrderRegel
        SET AantalBetaald = AantalBetaald + @Aantal
        WHERE Id = @OrderRegelId;
    ";

            var wijzigingen = items
                .Where(i => i.SelectieAantal > 0)
                .Select(i => new
                {
                    OrderRegelId = i.OrderRegelId,
                    Aantal = i.SelectieAantal
                })
                .ToList();

            if (!wijzigingen.Any())
                return;

            using var connection = dbConnectionProvider.GetDatabaseConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            foreach (var wijziging in wijzigingen)
            {
                await connection.ExecuteAsync(sql, wijziging, transaction);
            }

            transaction.Commit();
        }

        public async Task UpdateAantalBetaaldAsync(Dictionary<int, int> wijzigingen)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            foreach (var (orderRegelId, extraAantal) in wijzigingen)
            {
                await connection.ExecuteAsync(
                    @"UPDATE OrderRegel 
              SET AantalBetaald = AantalBetaald + @Extra 
              WHERE Id = @Id",
                    new { Id = orderRegelId, Extra = extraAantal });
            }
        }  
    }
}