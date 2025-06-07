
public class Bestelling
{
    public int Id { get; set; }
    public string Kostenplaats { get; set; }
    public DateTime Tijdstip { get; set; }

    public int OberId { get; set; }
    public Ober Ober { get; set; }

    public int TafelId { get; set; }
    public Tafel Tafel { get; set; }

    public ICollection<Ronde> Rondes { get; set; }
}
