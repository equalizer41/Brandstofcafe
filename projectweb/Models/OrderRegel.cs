public class OrderRegel
{
    public int Id { get; set; }
    public int Aantal { get; set; }
    public int AantalBetaald { get; set; }
    public int ProductId { get; set; }
    public int RondeId { get; set; }
    public Product? Product { get; set; }
    public List<OrderRegelAddOn> AddOns { get; set; } = new();

}
