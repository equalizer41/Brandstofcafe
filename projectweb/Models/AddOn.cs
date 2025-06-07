
public class AddOn
{
    public int Id { get; set; }
    public string Naam { get; set; }
    public int AddOnCategorieId { get; set; }
    public AddOnCategorie AddOnCategorie { get; set; }
}
