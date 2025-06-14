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
                        AND IsBetaald = 0

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

            //  Eerst uitvoeren om cache te vullen
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
         

            // Maak nieuwe ronde op basis van origineel
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

            // Sla nieuwe ronde + regels + addons op in DB
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
                        // Hoofdregel (product)
                        result.Add(new RekeningItem
                        {
                            BestellingId = bestelling.Id,
                            RondeNr = ronde.RondNr,
                            OrderRegelId = regel.Id,
                            ProductNaam = regel.Product?.Naam ?? "Onbekend",
                            Aantal = regel.Aantal,
                            AantalBetaald = regel.AantalBetaald,
                            PrijsPerStuk = regel.Product?.Prijs ?? 0,
                            IsAddOn = false,
                            HoofdregelId = null
                        });

                        // AddOns
                        foreach (var addon in regel.AddOns)
                        {
                            result.Add(new RekeningItem
                            {
                                BestellingId = bestelling.Id,
                                RondeNr = ronde.RondNr,
                                OrderRegelId = addon.Id, // eigen id
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

        public async Task UpdateBetaalStatusAlsVolledigBetaaldAsync(int bestellingId)
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();

            var onbetaaldAantal = await conn.ExecuteScalarAsync<int>(@"
        SELECT COUNT(*) FROM OrderRegel
        WHERE RondeId IN (
            SELECT Id FROM Ronde WHERE BestellingId = @BestellingId
        )
        AND AantalBetaald < Aantal
    ", new { BestellingId = bestellingId });

            if (onbetaaldAantal == 0)
            {
                await conn.ExecuteAsync(
                    "UPDATE Bestelling SET IsBetaald = 1 WHERE Id = @BestellingId",
                    new { BestellingId = bestellingId });
            }
        }

        public async Task<Bestelling?> HaalBestellingAsync(int bestellingId)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return await connection.QueryFirstOrDefaultAsync<Bestelling>(
                "SELECT * FROM Bestelling WHERE Id = @Id",
                new { Id = bestellingId });
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

        public async Task<Bestelling> MaakNieuweBestellingMetRondeAsync(int tafelId, int oberId = 1)
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            var bestelling = new Bestelling
            {
                TafelId = tafelId,
                Tijdstip = DateTime.Now,
                OberID = oberId,
                Rondes = new List<Ronde>()
            };

            bestelling.Id = await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO Bestelling (TafelId, Tijdstip, OberID, IsBetaald) 
          VALUES (@TafelId, @Tijdstip, @OberID, 0);
          SELECT CAST(SCOPE_IDENTITY() as int);",
                bestelling, transaction);

            var ronde = new Ronde
            {
                BestellingId = bestelling.Id,
                Tijdstip = DateTime.Now,
                Status = StatusEnum.Besteld,
                RondNr = 1,
                OrderRegels = new List<OrderRegel>()
            };

            ronde.Id = await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO Ronde (BestellingId, Tijdstip, Status, RondNr)
          VALUES (@BestellingId, @Tijdstip, @Status, @RondNr);
          SELECT CAST(SCOPE_IDENTITY() as int);",
                ronde, transaction);

            bestelling.Rondes.Add(ronde);

            transaction.Commit();
            return bestelling;
        }
        public List<RekeningItem> HaalRekeningItemsStructuurVoorBestelling(int bestellingId)
        {
            var bestelling = HaalVolledigeBestelling(bestellingId);

            var result = new List<RekeningItem>();

            foreach (var ronde in bestelling.Rondes)
            {
                foreach (var regel in ronde.OrderRegels)
                {
                    result.Add(new RekeningItem
                    {
                        BestellingId = bestelling.Id,
                        RondeNr = ronde.RondNr,
                        OrderRegelId = regel.Id,
                        ProductNaam = regel.Product?.Naam ?? "Onbekend",
                        Aantal = regel.Aantal,
                        AantalBetaald = regel.AantalBetaald,
                        PrijsPerStuk = regel.Product?.Prijs ?? 0,
                        IsAddOn = false,
                        HoofdregelId = null
                    });

                    foreach (var addon in regel.AddOns)
                    {
                        result.Add(new RekeningItem
                        {
                            BestellingId = bestelling.Id,
                            RondeNr = ronde.RondNr,
                            OrderRegelId = addon.Id,
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

            return result;
        }
        public async Task<Ronde> VoegNieuweRondeToeAsync(int bestellingId)
        {
            // Verkrijg de laatste ronde van de bestelling
            using var conn = dbConnectionProvider.GetDatabaseConnection();
            conn.Open();

            // Haal de laatste ronde op om het ronde nummer te verhogen
            var sqlLastRonde = @"
        SELECT TOP 1 * 
        FROM Ronde 
        WHERE BestellingId = @BestellingId 
        ORDER BY RondNr DESC";

            var laatsteRonde = await conn.QueryFirstOrDefaultAsync<Ronde>(sqlLastRonde, new { BestellingId = bestellingId });

            if (laatsteRonde == null)
            {
                throw new InvalidOperationException("Er is geen bestaande ronde voor deze bestelling.");
            }

            // Verkrijg het nieuwe ronde nummer
            int nieuwRondeNummer = laatsteRonde.RondNr + 1;

            // Maak de nieuwe ronde
            var nieuweRonde = new Ronde
            {
                RondNr = nieuwRondeNummer,
                BestellingId = bestellingId,
                Tijdstip = DateTime.Now,
                Status = StatusEnum.Besteld // Afhankelijk van je logica
            };

            // Insert de nieuwe ronde in de database
            var sqlInsertRonde = @"
        INSERT INTO Ronde (RondNr, Tijdstip, Status, BestellingId)
        VALUES (@RondNr, @Tijdstip, @Status, @BestellingId);
        SELECT CAST(SCOPE_IDENTITY() as int);";

            nieuweRonde.Id = await conn.ExecuteScalarAsync<int>(sqlInsertRonde, nieuweRonde);

            return nieuweRonde;
        }

        public async Task VoegOrderRegelToeAsync(OrderRegel regel)
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();

            // 1. Controleer of de opgegeven RondeId bestaat in de Ronde tabel
            var rondeBestaat = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Ronde WHERE Id = @RondeId",
                new { RondeId = regel.RondeId }
            );

            if (rondeBestaat == 0)
            {
                throw new InvalidOperationException("De opgegeven RondeId bestaat niet in de Ronde tabel.");
            }

            // 2. Voeg de OrderRegel toe aan de OrderRegel tabel
            regel.Id = await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO OrderRegel (ProductId, Aantal, AantalBetaald, RondeId)
          VALUES (@ProductId, @Aantal, @AantalBetaald, @RondeId);
          SELECT CAST(SCOPE_IDENTITY() as int);",
                regel
            );

            // 3. Voeg de AddOns toe aan de OrderRegelAddOn tabel
            foreach (var addon in regel.AddOns)
            {
                await conn.ExecuteAsync(
                    @"INSERT INTO OrderRegelAddOn (OrderRegelId, ProductAddOnId)
              VALUES (@OrderRegelId, @ProductAddOnId);",
                    new
                    {
                        OrderRegelId = regel.Id,
                        ProductAddOnId = addon.ProductAddOn.Id
                    }
                );
            }
        }



        public Bestelling? HaalVolledigeBestelling(int bestellingId)
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

WHERE b.Id = @BestellingId

ORDER BY b.Id, r.Id, o.Id, oa.Id;
";

            Bestelling? bestelling = null;

            var result = connection.Query<Bestelling, Ronde, OrderRegel, Product, OrderRegelAddOn, ProductAddOn, AddOn, Bestelling>(
                sql,
                (b, r, o, p, oa, pa, a) =>
                {
                    if (bestelling == null)
                    {
                        bestelling = b;
                    }

                    var bestaandeRonde = bestelling.Rondes.FirstOrDefault(x => x.Id == r.Id);
                    if (bestaandeRonde == null)
                    {
                        bestaandeRonde = r;
                        bestelling.Rondes.Add(bestaandeRonde);
                    }

                    var bestaandeRegel = bestaandeRonde.OrderRegels.FirstOrDefault(x => x.Id == o.Id);
                    if (bestaandeRegel == null)
                    {
                        bestaandeRegel = o;
                        bestaandeRegel.Product = p;
                        bestaandeRegel.AddOns = new();
                        bestaandeRonde.OrderRegels.Add(bestaandeRegel);
                    }

                    if (oa != null && oa.Id != 0)
                    {
                        if (pa != null)
                            oa.ProductAddOn = pa;

                        if (a != null && pa != null)
                            pa.AddOn = a;

                        bestaandeRegel.AddOns.Add(oa);
                    }

                    return bestelling;
                },
                param: new { BestellingId = bestellingId },
                splitOn: "Id,Id,Id,Id,Id"
            );

            return bestelling;
        }

    }
}