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

    public void UpdateAantalOrderRegel(OrderRegel regel, int nieuwAantal)
    {
        if (nieuwAantal > 0)
        {
            // Zoek de regel in de lijst van OrderRegelsInOpbouw
            var orderRegel = OrderRegelsInOpbouw.FirstOrDefault(r => r.ProductId == regel.ProductId &&
                                                                    r.AddOns.SequenceEqual(regel.AddOns));

            if (orderRegel != null)
            {
                // Werk het aantal bij van de juiste orderregel
                orderRegel.Aantal = nieuwAantal;
            }

            NotifyStateChanged();  // Update de UI
        }
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

    public async Task BevestigEnBewaarProductAsync(BestellingRepository repo, int aantal)
    {
        if (GeselecteerdProduct is null || ActieveRonde is null)
            return;

        // Zoek of er al een OrderRegel voor dit product bestaat
        var regel = OrderRegelsInOpbouw.FirstOrDefault(r => r.ProductId == GeselecteerdProduct.Id);

        if (regel != null)
        {
            // Werk het aantal bij in de bestaande regel
            regel.Aantal = aantal;
        }
        else
        {
            // Maak een nieuwe OrderRegel als deze nog niet bestaat
            regel = new OrderRegel
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
        }

        // Voeg de orderregel toe aan de database via de repository
        await repo.VoegOrderRegelToeAsync(regel);

        // Voeg de regel toe aan de in-memory lijst
        if (!OrderRegelsInOpbouw.Contains(regel))
        {
            OrderRegelsInOpbouw.Add(regel);
        }

        // Reset het geselecteerde product en add-ons
        GeselecteerdProduct = null;
        GeselecteerdeAddOns.Clear();

        NotifyStateChanged();  
    }


    // Betalingsbeheer
    public void SelecteerAlleProducten()
    {
        // Doorloop alle RekeningItems
        foreach (var item in RekeningItems)
        {
            // Controleer of het item nog te betalen is
            if (item.NogTeBetalen > 0)
            {
                // Stel de selectie voor dit item in
                item.SelectieAantal = item.Aantal - item.AantalBetaald;

                // Doorloop de RekeningItems opnieuw om de add-ons van het huidige item te selecteren
                foreach (var addon in RekeningItems)
                {
                    if (addon.IsAddOn && addon.HoofdregelId == item.OrderRegelId)
                    {
                        // Stel de selectie voor de add-ons in
                        addon.SelectieAantal = item.SelectieAantal;
                    }
                }
            }
        }

        NotifyStateChanged();
    }


    public List<(string ProductNaam, int TotaalAantal, List<string> AddOnNamen)> GroepeerSelectiesVoorOverzicht()
    {
        //  een lijst om het resultaat op te slaan
        List<(string ProductNaam, int TotaalAantal, List<string> AddOnNamen)> resultaat = new List<(string ProductNaam, int TotaalAantal, List<string> AddOnNamen)>();

        // Eerst filteren van de RekeningItems
        List<RekeningItem> gefilterdeItems = new List<RekeningItem>();
        foreach (var item in RekeningItems)
        {
            if (!item.IsAddOn && item.SelectieAantal > 0)
            {
                gefilterdeItems.Add(item);
            }
        }

        // Groeperen op ProductNaam
        Dictionary<string, List<RekeningItem>> gegroepeerdOpProductNaam = new Dictionary<string, List<RekeningItem>>();
        foreach (var item in gefilterdeItems)
        {
            if (!gegroepeerdOpProductNaam.ContainsKey(item.ProductNaam))
            {
                gegroepeerdOpProductNaam[item.ProductNaam] = new List<RekeningItem>();
            }
            gegroepeerdOpProductNaam[item.ProductNaam].Add(item);
        }

        // Voor elke groep het totaal aantal en de add-ons verzamelen
        foreach (var groep in gegroepeerdOpProductNaam)
        {
            string productNaam = groep.Key;
            int totaalAantal = 0;
            List<string> addOnNamen = new List<string>();

            // Bereken het totaal aantal voor dit product
            foreach (var item in groep.Value)
            {
                totaalAantal += item.SelectieAantal;

                // Zoek add-ons die bij dit product horen
                foreach (var addOn in RekeningItems)
                {
                    if (addOn.IsAddOn && addOn.HoofdregelId == item.OrderRegelId)
                    {
                        addOnNamen.Add($"{item.SelectieAantal} x {addOn.ProductNaam}");
                    }
                }
            }

            // Voeg de gegevens toe aan de resultaatlijst
            resultaat.Add((ProductNaam: productNaam, TotaalAantal: totaalAantal, AddOnNamen: addOnNamen));
        }

        return resultaat;
    }



    public async Task BetaalGeselecteerdeAsync(BestellingRepository repo)
    {
        // Maak een lege dictionary om de wijzigingen op te slaan
        Dictionary<int, int> wijzigingen = new Dictionary<int, int>();

        // Doorloop RekeningItems en voeg de geselecteerde items toe aan de dictionary
        foreach (var item in RekeningItems)
        {
            if (item.SelectieAantal > 0)
            {
                wijzigingen.Add(item.OrderRegelId, item.SelectieAantal);
            }
        }

        // Controleer of er wijzigingen zijn en of er een geselecteerde tafel is
        if (wijzigingen.Count > 0 && GeselecteerdeTafel != null)
        {
            // Werk het aantal betaalde items bij in de repository
            await repo.UpdateAantalBetaaldAsync(wijzigingen);

            // Haal de bijgewerkte rekeningitems voor de geselecteerde tafel op
            RekeningItems = repo.HaalRekeningItemsStructuur(GeselecteerdeTafel.Id);

            // Reset de selectievelden voor elk item
            foreach (var item in RekeningItems)
            {
                item.SelectieAantal = 0;
            }

            // Update de actieve bestelling als die bestaat
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
        decimal totaal = 0;

        // Doorloop RekeningItems en bereken het totaal voor geselecteerde items
        foreach (var item in RekeningItems)
        {
            if (!item.IsAddOn && item.SelectieAantal > 0)
            {
                totaal += item.SelectieAantal * item.TotaalPrijsInclusiefAddOns(RekeningItems);
            }
        }

        return totaal;
    }

    public decimal BerekenTotaalNogTeBetalen()
    {
        decimal totaal = 0;

        // Doorloop RekeningItems en bereken het totaal voor nog te betalen items
        foreach (var item in RekeningItems)
        {
            if (!item.IsAddOn)
            {
                decimal resterendAantal = item.Aantal - item.AantalBetaald;
                totaal += resterendAantal * item.TotaalPrijsInclusiefAddOns(RekeningItems);
            }
        }

        return totaal;
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
