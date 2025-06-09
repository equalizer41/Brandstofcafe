
using projectweb.Models;

public class Product
{
    public int Id { get; set; }
    public required string Naam { get; set; }
    public decimal Prijs { get; set; }

    public int CategorieId { get; set; }
    public required Categorie Categorie { get; set; }

    public required ICollection<ProductAddOnCategorie> AddOnCategorieen { get; set; }
    public required ICollection<ProductAddOn> AddOns { get; set; }
}
