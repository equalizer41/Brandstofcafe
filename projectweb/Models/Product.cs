
public class Product
{
    public int Id { get; set; }
    public required string Naam { get; set; }
    public decimal Prijs { get; set; }

    public int CategorieId { get; set; }
    public required Categorie Categorie { get; set; }
    public ICollection<ProductAddOnCategorie> AddOnCategorieen { get; set; } = new List<ProductAddOnCategorie>();
    public required ICollection<ProductAddOn> AddOns { get; set; }
}
