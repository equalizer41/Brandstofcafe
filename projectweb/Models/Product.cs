
public class Product
{
    public int Id { get; set; }
    public string Naam { get; set; }
    public decimal Prijs { get; set; }

    public int CategorieId { get; set; }
    public Categorie Categorie { get; set; }

    public ICollection<ProductAddOnCategorie> AddOnCategorieen { get; set; }
    public ICollection<ProductAddOn> AddOns { get; set; }
}
