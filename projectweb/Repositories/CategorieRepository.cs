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

            // Haal de lijst van alle categorieën op uit de database
            var lijst = conn.Query<Categorie>("SELECT Id, Naam, OuderCategorieId FROM Categorie").ToList();

            // Zet alles in een dictionary voor gemakkelijke toegang
            var dict = lijst.ToDictionary(c => c.Id);

            // Koppel de subcategorieën aan hun oudercategorieën
            foreach (var cat in lijst)
            {
                cat.SubCategorieen = new List<Categorie>();

                // Koppel de categorieën met hun oudercategorieën
                if (cat.OuderCategorieId != null && dict.TryGetValue(cat.OuderCategorieId.Value, out var ouder))
                {
                    ouder.SubCategorieen.Add(cat);
                    cat.OuderCategorie = ouder;
                }
            }

            // Verzamel de topniveau categorieën (waar OuderCategorieId null is)
            var topNiveauCategorieen = new List<Categorie>();
            foreach (var cat in lijst)
            {
                if (cat.OuderCategorieId == null)
                {
                    topNiveauCategorieen.Add(cat);
                }
            }

            // Retourneer alleen de topniveau categorieën
            return topNiveauCategorieen;
        }





    }
}
