namespace projectweb.Models
{
    public class ProductAddOn
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int AddOnId { get; set; }
        public AddOn AddOn { get; set; }

        public decimal Prijs { get; set; }
    }
}