using Dapper;
using projectweb.Components;
using System;


namespace projectweb.Repositories
{
    public class SectieRepository
    {
        private readonly DbConnectionProvider dbConnectionProvider;

        public SectieRepository(DbConnectionProvider dbConnectionProvider)
        {
            this.dbConnectionProvider = dbConnectionProvider;
        }
        public List<Sectie> GetAll()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<Sectie>("SELECT * FROM Sectie").ToList();
        }

        public Sectie GetById(int id)
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();

            var sql = "SELECT * FROM Sectie WHERE Id = @Id";
            return connection.QueryFirstOrDefault<Sectie>(sql, new { Id = id });
        }

    }
}