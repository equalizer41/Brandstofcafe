using Dapper;

namespace projectweb.Repositories
{
    public class TafelRepository
    {

        private readonly DbConnectionProvider dbConnectionProvider;

        public TafelRepository(DbConnectionProvider dbConnectionProvider)
        {
            this.dbConnectionProvider = dbConnectionProvider;
        }
        public class TafelOverzicht
        {
            public Tafel Tafel { get; set; }
            public Bestelling ActieveBestelling { get; set; }
        }

        public List<Tafel> GetAll()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();

            var sql = @"
        SELECT t.Id, t.Naam, t.SectieId,
               s.Id, s.Naam
        FROM Tafel t
        JOIN Sectie s ON t.SectieId = s.Id";

            var result = connection.Query<Tafel, Sectie, Tafel>(
                sql,
                (tafel, sectie) =>
                {
                    tafel.Sectie = sectie;
                    return tafel;
                },
                splitOn: "Id" // eerste kolom van tweede object
            );

            return result.ToList();
        }


        public async Task<List<TafelOverzicht>> GetTafelOverzichtAsync()
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();

            var tafels = await conn.QueryAsync<Tafel>("SELECT * FROM Tafel");

            var sql = @"
            SELECT b.*, r.*, o.*
            FROM Bestelling b
            LEFT JOIN Ronde r ON r.BestellingId = b.Id
            LEFT JOIN OrderRegel o ON o.RondeId = r.Id
            WHERE b.IsBetaald = 0"; // Of bepaal dat via de logica, afhankelijk van je model

            var bestellingDict = new Dictionary<int, Bestelling>();

            await conn.QueryAsync<Bestelling, Ronde, OrderRegel, Bestelling>(
                sql,
                (bestelling, ronde, regel) =>
                {
                    if (!bestellingDict.TryGetValue(bestelling.Id, out var b))
                    {
                        b = bestelling;
                        b.Rondes = new List<Ronde>();
                        bestellingDict.Add(b.Id, b);
                    }

                    if (ronde != null)
                    {
                        var bestaandeRonde = b.Rondes.FirstOrDefault(r => r.Id == ronde.Id);
                        if (bestaandeRonde == null)
                        {
                            ronde.OrderRegels = new List<OrderRegel>();
                            b.Rondes.Add(ronde);
                            bestaandeRonde = ronde;
                        }

                        if (regel != null)
                        {
                            bestaandeRonde.OrderRegels.Add(regel);
                        }
                    }

                    return b;
                },
                splitOn: "Id,Id"
            );

            var result = tafels.Select(tafel =>
            {
                var bestelling = bestellingDict.Values.FirstOrDefault(b => b.TafelId == tafel.Id);
                return new TafelOverzicht
                {
                    Tafel = tafel,
                    ActieveBestelling = bestelling
                };
            }).ToList();

            return result;
        }
    }
}
