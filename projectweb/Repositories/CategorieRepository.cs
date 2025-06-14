using Dapper;

namespace projectweb.Repositories
{
    public class CategorieRepository
    {
        private readonly DbConnectionProvider dbConnectionProvider;

        public CategorieRepository(DbConnectionProvider dbConnectionProvider)
        {
            this.dbConnectionProvider = dbConnectionProvider;
        }

        public List<Categorie> GetAll()
        {
            using var connection = dbConnectionProvider.GetDatabaseConnection();
            return connection.Query<Categorie>("SELECT * FROM Categorie").ToList();
        }
        public List<Categorie> GetCategorieBoom()
        {
            using var conn = dbConnectionProvider.GetDatabaseConnection();

            // Stap 1: haal vlakke lijst op uit de database
            var lijst = conn.Query<Categorie>("SELECT Id, Naam, OuderCategorieId FROM Categorie").ToList();

            // Stap 2: zet alles in een dictionary
            var dict = lijst.ToDictionary(c => c.Id);

            // Stap 3: koppel de subcategorieën
            foreach (var cat in lijst)
            {
                cat.SubCategorieen = new List<Categorie>();

                if (cat.OuderCategorieId != null && dict.TryGetValue(cat.OuderCategorieId.Value, out var ouder))
                {
                    ouder.SubCategorieen.Add(cat);
                    cat.OuderCategorie = ouder;
                }
            }

            // Stap 4: return topniveau (dus waar geen OuderCategorieId is)
            return lijst.Where(c => c.OuderCategorieId == null).ToList();
        }




    }
}
