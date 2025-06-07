public class Bestelling
{
    public int Id { get; set; }
    public DateTime Tijdstip { get; set; }

    public int? OberID { get; set; }
    public int TafelId { get; set; }
    public ICollection<Ronde> Rondes { get; set; } = new List<Ronde>();
    public bool IsBetaald { get; set; }
}
