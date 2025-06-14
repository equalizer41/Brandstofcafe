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
        public List<Tafel> GetAll(int sectieId)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();

            var sql = @"
        SELECT t.Id, t.Naam, t.SectieId,
               s.Id AS SectieId, s.Naam AS SectieNaam
        FROM Tafel t
        JOIN Sectie s ON t.SectieId = s.Id
        WHERE t.SectieId = @SectieId";

            //  koppel de resultaten aan Tafel en Sectie objecten
            var result = connection.Query<Tafel, Sectie, Tafel>(
                sql,
                (tafel, sectie) =>
                {
                    tafel.Sectie = sectie;
                    return tafel;
                },
                new { SectieId = sectieId }, 
                splitOn: "SectieId"
            );

            return result.ToList();
        }
        public Tafel GetById(int id)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            var sql = "SELECT * FROM Tafel WHERE Id = @Id";
            return connection.QueryFirstOrDefault<Tafel>(sql, new { Id = id });
        }

    }
}
