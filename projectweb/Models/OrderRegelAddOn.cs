
using projectweb.Models;

public class OrderRegelAddOn
{
    public int Id { get; set; }
    public int OrderRegelId { get; set; }
    public OrderRegel OrderRegel { get; set; }

    public int ProductAddOnId { get; set; }
    public ProductAddOn ProductAddOn { get; set; }
}
