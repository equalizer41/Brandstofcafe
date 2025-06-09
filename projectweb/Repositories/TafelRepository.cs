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
     
    }
}
