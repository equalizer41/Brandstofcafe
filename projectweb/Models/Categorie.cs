public class Categorie
{
    public Categorie()
    {
       SubCategorieen = new List<Categorie>();

    }
    public int Id { get; set; }
    public string Naam { get; set; }
    public int? OuderCategorieId { get; set; }
    public Categorie OuderCategorie { get; set; }
    public ICollection<Categorie> SubCategorieen { get; set; }
}
