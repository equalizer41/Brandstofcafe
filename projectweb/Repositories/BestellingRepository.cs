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

        public async Task<int> CreateAsync(Bestelling bestelling)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Insert Bestelling, inclusief optionele OberID
                var sqlBestelling = @"
        INSERT INTO Bestelling (TafelId, Tijdstip, IsBetaald, OberID)
        VALUES (@TafelId, @Tijdstip, 0, @OberID);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var bestellingId = await connection.ExecuteScalarAsync<int>(sqlBestelling, new
                {
                    bestelling.TafelId,
                    bestelling.Tijdstip,
                    bestelling.OberID  // ✅ Wordt null als niet meegegeven
                }, transaction);

                bestelling.Id = bestellingId;

                // 2. Voeg eerste Ronde toe
                var eersteRonde = new Ronde
                {
                    BestellingId = bestellingId,
                    RondNr = 1,
                    Tijdstip = DateTime.Now,
                    Status = StatusEnum.Besteld
                };

                var sqlRonde = @"
        INSERT INTO Ronde (BestellingId, RondNr, Tijdstip, Status)
        VALUES (@BestellingId, @RondNr, @Tijdstip, @Status);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var rondeId = await connection.ExecuteScalarAsync<int>(sqlRonde, new
                {
                    eersteRonde.BestellingId,
                    eersteRonde.RondNr,
                    eersteRonde.Tijdstip,
                    Status = (int)eersteRonde.Status
                }, transaction);

                eersteRonde.Id = rondeId;
                bestelling.Rondes = new List<Ronde> { eersteRonde };

                transaction.Commit();
                return bestellingId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB FOUT] {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                transaction.Rollback();
                throw;
            }
        }

        public List<Bestelling> GetByTafelId(int tafelId)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();

            var bestellingen = connection.Query<Bestelling>(
                "SELECT * FROM Bestelling WHERE TafelId = @TafelId",
                new { TafelId = tafelId }).ToList();

            foreach (var bestelling in bestellingen)
            {
                // Voeg rondes toe
                var rondes = connection.Query<Ronde>(
                    "SELECT * FROM Ronde WHERE BestellingId = @BestellingId",
                    new { BestellingId = bestelling.Id }).ToList();

                foreach (var ronde in rondes)
                {
                    // Voeg orderregels toe
                    var orderRegels = connection.Query<OrderRegel>(
                        "SELECT * FROM OrderRegel WHERE RondeId = @RondeId",
                        new { RondeId = ronde.Id }).ToList();

                    ronde.OrderRegels = orderRegels;
                    Console.WriteLine($"Bestelling {bestelling.Id} → {rondes.Count} rondes");

                }

                bestelling.Rondes = rondes;
            }

            return bestellingen;
        }

        public List<Bestelling> GetOnbetaaldeBestellingenVoorTafel(int tafelId)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            var sql = "SELECT * FROM Bestelling WHERE TafelId = @TafelId AND IsBetaald = 0";
            return connection.Query<Bestelling>(sql, new { TafelId = tafelId }).ToList();
        }


    
    }
}