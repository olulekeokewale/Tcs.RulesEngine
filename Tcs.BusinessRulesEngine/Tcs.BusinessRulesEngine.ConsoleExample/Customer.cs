
namespace Tcs.BusinessRulesEngine.ConsoleExample;


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

// Example domain models
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public string MembershipLevel { get; set; }
    public decimal TotalPurchases { get; set; }
    public DateTime LastPurchaseDate { get; set; }
    public bool IsVip { get; set; }
    public string Status { get; set; } = "Active";

    public void UpgradeToVip ()
    {
        IsVip = true;
        MembershipLevel = "VIP";
        Console.WriteLine($"Customer {Name} upgraded to VIP status!");
    }

    public void SendWelcomeEmail ()
    {
        Console.WriteLine($"Welcome email sent to {Name} at {Email}");
    }
}
