using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcs.BusinessRulesEngine.ConsoleExample;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public bool HasDiscount { get; set; }
    public decimal DiscountAmount { get; set; }

    public void ApplyDiscount ( decimal percentage )
    {
        DiscountAmount = Amount * (percentage / 100);
        HasDiscount = true;
        Console.WriteLine($"Applied {percentage}% discount to order {Id}: ${DiscountAmount}");
    }
}