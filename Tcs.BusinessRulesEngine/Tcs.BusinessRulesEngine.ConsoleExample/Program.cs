using Microsoft.Extensions.DependencyInjection;
using Tcs.BusinessRulesEngine;
using Tcs.BusinessRulesEngine.ConsoleExample;
using Tcs.BusinessRulesEngine.Rules;
public class Program
{
    public static async Task Main ( string[] args )
{
    // Setup dependency injection
    var services = new ServiceCollection();

    // Add Entity Framework with in-memory database for demo
    services.AddRulesEngine(options =>
        options.UseInMemoryDatabase("RulesEngineDemo"));

    var serviceProvider = services.BuildServiceProvider();

    // Initialize the database with sample rules
    await InitializeRules(serviceProvider);

    // Demo the rules engine
    await RunDemo(serviceProvider);
}

private static async Task InitializeRules ( IServiceProvider serviceProvider )
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<RulesEngineContext>();
    var rulesEngine = scope.ServiceProvider.GetRequiredService<IRulesEngine>();

    await context.Database.EnsureCreatedAsync();

    // Rule 1: VIP Customer Upgrade Rule
    var vipRule = new Tcs.BusinessRulesEngine.Rules.Rule
    {
        Name = "VIP Customer Upgrade",
        Description = "Upgrade customers to VIP when they have high total purchases",
        TargetType = "Customer",
        Priority = 10
    };

    vipRule.Conditions.Add(new RuleCondition
    {
        PropertyName = "TotalPurchases",
        Operator = "GreaterThanOrEqual",
        Value = "1000",
        SequenceOrder = 1
    });

    vipRule.Conditions.Add(new RuleCondition
    {
        PropertyName = "IsVip",
        Operator = "Equals",
        Value = "False",
        LogicalOperator = "AND",
        SequenceOrder = 2
    });

    vipRule.Actions.Add(new RuleAction
    {
        ActionType = "SetProperty",
        Target = "IsVip",
        Parameters = "True",
        ExecutionOrder = 1
    });

    vipRule.Actions.Add(new RuleAction
    {
        ActionType = "InvokeMethod",
        Target = "UpgradeToVip",
        ExecutionOrder = 2
    });

    await rulesEngine.CreateRuleAsync(vipRule);

    // Rule 2: New Customer Welcome Rule
    var welcomeRule = new Tcs.BusinessRulesEngine.Rules.Rule
    {
        Name = "New Customer Welcome",
        Description = "Send welcome email to new customers",
        TargetType = "Customer",
        Priority = 5
    };

    welcomeRule.Conditions.Add(new RuleCondition
    {
        PropertyName = "TotalPurchases",
        Operator = "Equals",
        Value = "0",
        SequenceOrder = 1
    });

    welcomeRule.Actions.Add(new RuleAction
    {
        ActionType = "InvokeMethod",
        Target = "SendWelcomeEmail",
        ExecutionOrder = 1
    });

    await rulesEngine.CreateRuleAsync(welcomeRule);

    // Rule 3: Large Order Discount Rule
    var discountRule = new Rule
    {
        Name = "Large Order Discount",
        Description = "Apply 10% discount for orders over $500",
        TargetType = "Order",
        Priority = 8
    };

    discountRule.Conditions.Add(new RuleCondition
    {
        PropertyName = "Amount",
        Operator = "GreaterThan",
        Value = "500",
        SequenceOrder = 1
    });

    discountRule.Conditions.Add(new RuleCondition
    {
        PropertyName = "HasDiscount",
        Operator = "Equals",
        Value = "False",
        LogicalOperator = "AND",
        SequenceOrder = 2
    });

    discountRule.Actions.Add(new RuleAction
    {
        ActionType = "InvokeMethod",
        Target = "ApplyDiscount",
        Parameters = "10", // This would be passed as method parameter
        ExecutionOrder = 1
    });

    await rulesEngine.CreateRuleAsync(discountRule);

    // Rule 4: Inactive Customer Reactivation Rule
    var reactivationRule = new Rule
    {
        Name = "Inactive Customer Reactivation",
        Description = "Mark customers as inactive if no recent purchases",
        TargetType = "Customer",
        Priority = 3
    };

    reactivationRule.Conditions.Add(new RuleCondition
    {
        PropertyName = "LastPurchaseDate",
        Operator = "LessThan",
        Value = DateTime.UtcNow.AddDays(-90).ToString(), // 3 months ago
        SequenceOrder = 1
    });

    reactivationRule.Conditions.Add(new RuleCondition
    {
        PropertyName = "Status",
        Operator = "Equals",
        Value = "Active",
        LogicalOperator = "AND",
        SequenceOrder = 2
    });

    reactivationRule.Actions.Add(new RuleAction
    {
        ActionType = "SetProperty",
        Target = "Status",
        Parameters = "Inactive",
        ExecutionOrder = 1
    });

    reactivationRule.Actions.Add(new RuleAction
    {
        ActionType = "LogMessage",
        Target = "",
        Parameters = "Customer marked as inactive due to no recent purchases",
        ExecutionOrder = 2
    });

    await rulesEngine.CreateRuleAsync(reactivationRule);

    Console.WriteLine("Sample rules created successfully!");
}

private static async Task RunDemo ( IServiceProvider serviceProvider )
{
    using var scope = serviceProvider.CreateScope();
    var rulesEngine = scope.ServiceProvider.GetRequiredService<IRulesEngine>();

    Console.WriteLine("\n=== Rules Engine Demo ===\n");

    // Demo 1: New customer (should trigger welcome email)
    Console.WriteLine("Demo 1: New Customer");
    var newCustomer = new Customer
    {
        Id = 1,
        Name = "John Doe",
        Email = "john@example.com",
        Age = 30,
        MembershipLevel = "Bronze",
        TotalPurchases = 0,
        LastPurchaseDate = DateTime.UtcNow
    };

    var result1 = await rulesEngine.EvaluateAsync(newCustomer);
    Console.WriteLine($"Matched Rules: {result1.MatchedRules.Count}");
    Console.WriteLine($"Executed Actions: {string.Join(", ", result1.ExecutedActions)}");
    Console.WriteLine();

    // Demo 2: High-value customer (should upgrade to VIP)
    Console.WriteLine("Demo 2: High-Value Customer");
    var highValueCustomer = new Customer
    {
        Id = 2,
        Name = "Jane Smith",
        Email = "jane@example.com",
        Age = 35,
        MembershipLevel = "Gold",
        TotalPurchases = 1500,
        LastPurchaseDate = DateTime.UtcNow,
        IsVip = false
    };

    var result2 = await rulesEngine.EvaluateAsync(highValueCustomer);
    Console.WriteLine($"Matched Rules: {result2.MatchedRules.Count}");
    Console.WriteLine($"Executed Actions: {string.Join(", ", result2.ExecutedActions)}");
    Console.WriteLine($"Customer VIP Status: {highValueCustomer.IsVip}");
    Console.WriteLine();

    // Demo 3: Large order (should get discount)
    Console.WriteLine("Demo 3: Large Order");
    var largeOrder = new Order
    {
        Id = 101,
        CustomerId = 2,
        Amount = 750,
        OrderDate = DateTime.UtcNow,
        Status = "Pending",
        HasDiscount = false
    };

    var result3 = await rulesEngine.EvaluateAsync(largeOrder);
    Console.WriteLine($"Matched Rules: {result3.MatchedRules.Count}");
    Console.WriteLine($"Executed Actions: {string.Join(", ", result3.ExecutedActions)}");
    Console.WriteLine($"Order Discount Applied: {largeOrder.HasDiscount}");
    Console.WriteLine($"Discount Amount: ${largeOrder.DiscountAmount}");
    Console.WriteLine();

    // Demo 4: Inactive customer
    Console.WriteLine("Demo 4: Inactive Customer");
    var inactiveCustomer = new Customer
    {
        Id = 3,
        Name = "Bob Johnson",
        Email = "bob@example.com",
        Age = 45,
        MembershipLevel = "Silver",
        TotalPurchases = 500,
        LastPurchaseDate = DateTime.UtcNow.AddDays(-120), // 4 months ago
        Status = "Active"
    };

    var result4 = await rulesEngine.EvaluateAsync(inactiveCustomer);
    Console.WriteLine($"Matched Rules: {result4.MatchedRules.Count}");
    Console.WriteLine($"Executed Actions: {string.Join(", ", result4.ExecutedActions)}");
    Console.WriteLine($"Customer Status: {inactiveCustomer.Status}");
    Console.WriteLine();

    // Demo 5: List all rules for Customer type
    Console.WriteLine("Demo 5: All Customer Rules");
    var customerRules = await rulesEngine.GetRulesForTypeAsync("Customer");
    foreach (var rule in customerRules)
    {
        Console.WriteLine($"- {rule.Name}: {rule.Description} (Priority: {rule.Priority})");
    }
}
}