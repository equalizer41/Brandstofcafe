
public enum StatusEnum { Besteld, Bereid, Uitgeleverd }

public class Ronde
{
    public int Id { get; set; }
    public int RondNr { get; set; }
    public DateTime Tijdstip { get; set; }

    public StatusEnum Status { get; set; }

    public int BestellingId { get; set; }
    public Bestelling Bestelling { get; set; }

    public ICollection<OrderRegel> OrderRegels { get; set; } = new List<OrderRegel>();
}
