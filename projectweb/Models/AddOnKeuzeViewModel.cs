public class AddOnKeuzeViewModel
{
    public AddOn AddOn { get; set; } = null!;
    public decimal Prijs { get; set; }
    public int Aantal { get; set; } = 1;
    public bool Geselecteerd { get; set; } = false;
    public int AddOnCategorieId { get; set; }

}
