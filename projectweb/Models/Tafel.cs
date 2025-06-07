
public class Tafel
{
    public int Id { get; set; }
    public int Naam { get; set; }
    public int SectieId { get; set; }
    public Sectie Sectie { get; set; }
    public ICollection<Bestelling> Bestellingen { get; set; }
}
