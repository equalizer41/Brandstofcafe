using projectweb;
using projectweb.Repositories;

public class BestellingStateService
{
    // Contextselectie
    public Sectie? GeselecteerdeSectie { get; private set; }
    public Tafel? GeselecteerdeTafel { get; private set; }
    public Bestelling? ActieveBestelling { get; private set; }
    public Ronde? ActieveRonde { get; private set; }

    // Navigatie en keuze
    public Product? GeselecteerdProduct { get; set; }

    // Order-in-opbouw
    public List<ProductAddOn> GeselecteerdeAddOns { get; private set; } = new();
    public List<OrderRegel> OrderRegelsInOpbouw { get; private set; } = new();

    // Betaling
    public List<Bestelling> TeBetalenBestellingen { get; private set; } = new();
    public List<RekeningItem> RekeningItems { get;  set; } = new();
    public List<OrderRegel> GeselecteerdeTeBetalenRegels { get; private set; } = new();


    public event Action? OnChange;

    // Sectie & Tafel selectie
    public void SelecteerSectie(Sectie sectie)
    {
        GeselecteerdeSectie = sectie;
        GeselecteerdeTafel = null;
        NotifyStateChanged();
        ClearContext();
    }

    public void SelecteerTafel(Tafel tafel)
    {
        GeselecteerdeTafel = tafel;
        ActieveBestelling = null;
        ActieveRonde = null;
        OrderRegelsInOpbouw.Clear();
        NotifyStateChanged();
    }

    // Bestelling en Ronde


    public void StartNieuweRonde(Ronde nieuweRonde)
    {
        ActieveRonde = nieuweRonde;
        NotifyStateChanged();
    }


    // Productkeuze
    public void SelecteerProduct(Product product)
    {
        GeselecteerdProduct = product;
        GeselecteerdeAddOns.Clear();
        NotifyStateChanged();
    }

    // In BestelStateService.cs
    public void SelecteerAddOn(ProductAddOn productAddOn)
    {
        // Voeg alleen toe als het nog niet bestaat
        if (!GeselecteerdeAddOns.Any(p => p.AddOnId == productAddOn.AddOnId))
        {
            // Zorg dat AddOn is gevuld!
            if (productAddOn.AddOn == null)
            {
                // (optioneel) gooi hier een exception/logging of haal 'm opnieuw op
            }

            GeselecteerdeAddOns.Add(productAddOn);
        }
    }




    public void VerwijderAddOn(AddOn addon)
    {
        GeselecteerdeAddOns.RemoveAll(pa => pa.AddOn.Id == addon.Id);
        NotifyStateChanged();
    }

    public async Task BevestigEnBewaarProductAsync(BestellingRepository repo, int aantal)
    {
        if (GeselecteerdProduct is null || ActieveRonde is null)
            return;

        var regel = new OrderRegel
        {
            ProductId = GeselecteerdProduct.Id,
            RondeId = ActieveRonde.Id,
            Aantal = aantal,
            AantalBetaald = 0,
            AddOns = GeselecteerdeAddOns.Select(pa => new OrderRegelAddOn
            {
                ProductAddOn = pa
            }).ToList()
        };

        await repo.VoegOrderRegelToeAsync(regel);

        OrderRegelsInOpbouw.Add(regel);
        GeselecteerdProduct = null;
        GeselecteerdeAddOns.Clear();
        NotifyStateChanged();
    }




    // Betalingsbeheer
    public void SelecteerAlleProducten()
    {
        foreach (var item in RekeningItems.Where(i => i.NogTeBetalen > 0))
        {
            item.SelectieAantal = item.Aantal - item.AantalBetaald;

            foreach (var addon in RekeningItems
                .Where(a => a.IsAddOn && a.HoofdregelId == item.OrderRegelId))
            {
                addon.SelectieAantal = item.SelectieAantal;
            }
        }

        NotifyStateChanged();
    }

    public List<(string ProductNaam, int TotaalAantal, List<string> AddOnNamen)> GroepeerSelectiesVoorOverzicht()
    {
        return RekeningItems
            .Where(i => !i.IsAddOn && i.SelectieAantal > 0)
            .Select(i => new
            {
                i.ProductNaam,
                i.SelectieAantal,
                AddOnNamen = RekeningItems
                    .Where(a => a.IsAddOn && a.HoofdregelId == i.OrderRegelId)
                    .Select(a => a.ProductNaam)
                    .ToList()
            })
            .GroupBy(x => x.ProductNaam)
            .Select(g => (
                ProductNaam: g.Key,
                TotaalAantal: g.Sum(x => x.SelectieAantal),
                AddOnNamen: g.SelectMany(x => x.AddOnNamen.Select(a => $"{x.SelectieAantal} x {a}")).ToList()
            ))
            .ToList();
    }


    public async Task BetaalGeselecteerdeAsync(BestellingRepository repo)
    {
        var wijzigingen = RekeningItems
            .Where(i => i.SelectieAantal > 0)
            .ToDictionary(i => i.OrderRegelId, i => i.SelectieAantal);

        if (wijzigingen.Any() && GeselecteerdeTafel != null)
        {
            await repo.UpdateAantalBetaaldAsync(wijzigingen);
            RekeningItems = repo.HaalRekeningItemsStructuur(GeselecteerdeTafel.Id);

            // Hier: reset selectievelden
            foreach (var item in RekeningItems)
            {
                item.SelectieAantal = 0;
            }

            // Actieve bestelling updaten indien nodig
            if (ActieveBestelling != null)
            {
                ActieveBestelling = await repo.HaalBestellingAsync(ActieveBestelling.Id);
            }

            NotifyStateChanged();
        }

    }

    public void UpdateSelectieAantal(RekeningItem hoofdregel, int nieuwAantal)
    {
        hoofdregel.SelectieAantal = nieuwAantal;

        foreach (var addon in RekeningItems
            .Where(a => a.IsAddOn && a.HoofdregelId == hoofdregel.OrderRegelId))
        {
            addon.SelectieAantal = nieuwAantal;
        }

        NotifyStateChanged();
    }

    public decimal BerekenGeselecteerdTotaal()
    {

        return RekeningItems
            .Where(i => !i.IsAddOn && i.SelectieAantal > 0)
            .Sum(item =>
                item.SelectieAantal * item.TotaalPrijsInclusiefAddOns(RekeningItems)
            );

    }

    public List<(RekeningItem Hoofdregel, List<RekeningItem> AddOns)> HaalGeselecteerdeProductenMetAddOns()
    {
        return RekeningItems
            .Where(i => !i.IsAddOn && i.SelectieAantal > 0)
            .Select(item => (
                item,
                RekeningItems
                    .Where(a => a.IsAddOn && a.HoofdregelId == item.OrderRegelId)
                    .ToList()
            ))
            .ToList();
    }

    public decimal BerekenTotaalNogTeBetalen()
    {
        return RekeningItems
            .Where(i => !i.IsAddOn)
            .Sum(i =>
                (i.Aantal - i.AantalBetaald) * i.TotaalPrijsInclusiefAddOns(RekeningItems)
            );
    }

    public void StartNieuweBestellingMetRonde(Bestelling bestelling, Ronde ronde)
    {
        ActieveBestelling = bestelling;
        ActieveRonde = ronde;
        OrderRegelsInOpbouw.Clear();
        GeselecteerdeAddOns.Clear();
        NotifyStateChanged();
    }

    // Interne tools
    private void ClearContext()
    {
        ActieveBestelling = null;
        ActieveRonde = null;
        OrderRegelsInOpbouw.Clear();
        GeselecteerdeAddOns.Clear();
        TeBetalenBestellingen.Clear();
    }

    public void Reset()
    {

        ClearContext();
        GeselecteerdeSectie = null;
        GeselecteerdeTafel = null;
        RekeningItems.Clear();
        GeselecteerdeTeBetalenRegels.Clear(); 
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
