public class RekeningItem
{
    public int BestellingId { get; set; }
    public int RondeNr { get; set; }
    public int OrderRegelId { get; set; }
    public string ProductNaam { get; set; } = "";
    public int Aantal { get; set; }
    public int AantalBetaald { get; set; }
    public decimal PrijsPerStuk { get; set; }
    public int SelectieAantal { get; set; } = 0;

    public bool IsAddOn { get; set; } = false;
    public string? AddOnNaam { get; set; }
    public int? HoofdregelId { get; set; }

    public decimal Subtotaal => Aantal * PrijsPerStuk;
    public decimal NogTeBetalen => (Aantal - AantalBetaald) * PrijsPerStuk;

    public decimal TotaalPrijsInclusiefAddOns(List<RekeningItem> items)
    {
        if (IsAddOn) return 0;

        var addons = items
            .Where(a => a.IsAddOn && a.HoofdregelId == OrderRegelId)
            .Sum(a => a.PrijsPerStuk);

        return PrijsPerStuk + addons;
    }

}
