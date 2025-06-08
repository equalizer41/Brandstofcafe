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
    }
}