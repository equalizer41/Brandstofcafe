
public class BestellingStateService
{
    // Contextselectie
    public Sectie? GeselecteerdeSectie { get; private set; }
    public Tafel? GeselecteerdeTafel { get; private set; }
    public Bestelling? ActieveBestelling { get; private set; }
    public Ronde? ActieveRonde { get; private set; }

    // Navigatie en keuze
    public Categorie? GeselecteerdeCategorie { get; set; }
    public Product? GeselecteerdProduct { get; set; }

    // Order-in-opbouw
    public List<AddOn> GeselecteerdeAddOns { get; private set; } = new();
    public List<OrderRegel> OrderRegelsInOpbouw { get; private set; } = new();

    // Betaling
    public List<Bestelling> TeBetalenBestellingen { get; private set; } = new();

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
    public void StartNieuweBestelling(Bestelling bestelling, Ronde ronde)
    {
        ActieveBestelling = bestelling;
        ActieveRonde = ronde;
        OrderRegelsInOpbouw.Clear();
        GeselecteerdeAddOns.Clear();
        NotifyStateChanged();
    }

    // Productkeuze
    public void SelecteerProduct(Product product)
    {
        GeselecteerdProduct = product;
        GeselecteerdeAddOns.Clear();
        NotifyStateChanged();
    }

    public void SelecteerAddOn(AddOn addon)
    {
        if (!GeselecteerdeAddOns.Any(a => a.Id == addon.Id))
            GeselecteerdeAddOns.Add(addon);
        NotifyStateChanged();
    }

    public void VerwijderAddOn(AddOn addon)
    {
        GeselecteerdeAddOns.RemoveAll(a => a.Id == addon.Id);
        NotifyStateChanged();
    }

    // Orderregel aanmaken
    public void VoegOrderRegelToe(int aantal)
    {
        if (GeselecteerdProduct is null || ActieveRonde is null)
            return;

        var regel = new OrderRegel
        {
            Aantal = aantal,
            AantalBetaald = 0,
            ProductId = GeselecteerdProduct.Id,
            RondeId = ActieveRonde.Id,
            AddOns = GeselecteerdeAddOns.Select(addon => new OrderRegelAddOn
            {
                ProductAddOn = GeselecteerdProduct.AddOns.First(pa => pa.AddOnId == addon.Id),
                ProductAddOnId = addon.Id
            }).ToList()
        };

        OrderRegelsInOpbouw.Add(regel);
        GeselecteerdeAddOns.Clear();
        NotifyStateChanged();
    }

    // Betalingsbeheer
    public void VoegTeBetalenToe(Bestelling bestelling)
    {
        if (!TeBetalenBestellingen.Contains(bestelling))
            TeBetalenBestellingen.Add(bestelling);
        NotifyStateChanged();
    }

    public void ClearTeBetalen()
    {
        TeBetalenBestellingen.Clear();
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

    private void NotifyStateChanged() => OnChange?.Invoke();
}
