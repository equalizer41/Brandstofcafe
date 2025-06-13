public class OrderRegelComposer
{
    public OrderRegel Compose(Product product, List<AddOnKeuzeViewModel> keuzes)
    {
        var addOns = keuzes
            .Where(k => k.Geselecteerd)
            .SelectMany(k => Enumerable.Range(0, k.Aantal).Select(_ => k))
            .Select(k =>
            {
                var pa = product.AddOns.FirstOrDefault(p => p.AddOnId == k.AddOn.Id);
                return new OrderRegelAddOn
                {
                    ProductAddOn = pa!
                };
            })
            .ToList();

        return new OrderRegel
        {
            ProductId = product.Id,
            Aantal = 1,
            AantalBetaald = 0,
            AddOns = addOns
        };
    }
}