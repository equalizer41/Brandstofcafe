
public class OrderRegel
{
    public int Id { get; set; }
    public int Aantal { get; set; }
    public int AantalBetaald { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int RondeId { get; set; }
    public Ronde Ronde { get; set; }

    public ICollection<OrderRegelAddOn> AddOns { get; set; }
}
