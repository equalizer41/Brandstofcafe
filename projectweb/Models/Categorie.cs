public class Categorie
{
  
    public int Id { get; set; }
    public string Naam { get; set; }
    public int Niveau { get; set; }

    public int? OuderCategorieId { get; set; }
    public Categorie OuderCategorie { get; set; }
    public List<Categorie> SubCategorieen { get; set; } = new();
}
