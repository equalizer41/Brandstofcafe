public class Bestelling
{
    public int Id { get; set; }
    public DateTime Tijdstip { get; set; }

    public int? OberID { get; set; }
    public int TafelId { get; set; }

    public ICollection<Ronde> Rondes { get; set; } = new List<Ronde>();

    public bool IsBetaald { get; private set; }
  
    public bool IsVolledigBetaald()
    {
        foreach (var ronde in Rondes)
        {
            foreach (var or in ronde.OrderRegels)
            {
                if (or.AantalBetaald < or.Aantal)
                    return false;
            }
        }
        return true;
    }

    public void UpdateBetaalStatus()
    {
        IsBetaald = IsVolledigBetaald();
    }
}
