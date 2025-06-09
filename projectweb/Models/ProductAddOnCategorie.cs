namespace projectweb.Models
{
    public class ProductAddOnCategorie
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int AddOnCategorieId { get; set; }
        public AddOnCategorie AddOnCategorie { get; set; }

        public bool Verplicht { get; set; }
    }
}