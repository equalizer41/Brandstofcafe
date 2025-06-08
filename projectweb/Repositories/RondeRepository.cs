using Dapper;
using System.Data;

namespace projectweb.Repositories
{
    public class RondeRepository
    {
        private readonly DbConnectionProvider dbConnectionProvider;

        public RondeRepository(DbConnectionProvider dbConnectionProvider)
        {
            this.dbConnectionProvider = dbConnectionProvider;
        }

        public List<Ronde> GetAll()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<Ronde>("SELECT * FROM Ronde").ToList();
        }
        public Ronde? GetById(int id)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.QueryFirstOrDefault<Ronde>(
                "SELECT * FROM Ronde WHERE Id = @Id", new { Id = id });
        }

        public List<Ronde> GetByBestellingId(int bestellingId)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<Ronde>(
                "SELECT * FROM Ronde WHERE BestellingId = @BestellingId",
                new { BestellingId = bestellingId }).ToList();
        }
        public async Task<int> CreateAsync(Ronde ronde)
        {
            const string sql = @"
                INSERT INTO Ronde (RondNr, Tijdstip, Status, BestellingId)
                VALUES (@RondNr, @Tijdstip, @Status, @BestellingId);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = dbConnectionProvider.GetDatabaseConnection();
            var parameters = new
            {
                ronde.RondNr,
                ronde.Tijdstip,
                Status = (int)ronde.Status,
                ronde.BestellingId
            };

            return await connection.QuerySingleAsync<int>(sql, parameters);
        }
    }
}
