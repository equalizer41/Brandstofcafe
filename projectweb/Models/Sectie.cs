
public class Sectie
{
    public int Id { get; set; }
    public string Naam { get; set; }
    public ICollection<Tafel> Tafels { get; set; }
}
