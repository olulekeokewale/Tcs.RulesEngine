# Tcs.RulesEngine
Database Rules Engine

A flexible, database-driven rules engine for .NET applications that allows you to create, manage, and execute business rules dynamically without code changes. Perfect for applications that need configurable business logic, automated decision-making, and dynamic workflows.
Features
✅ Database-Driven: Store rules in your database using Entity Framework
✅ Generic & Type-Safe: Works with any C# object using reflection and strong typing
✅ Flexible Conditions: Support for complex boolean expressions with AND/OR logic
✅ Multiple Actions: Execute various actions when rules match (set properties, invoke methods, send notifications)
✅ Extensible Architecture: Easy to add custom operators and actions
✅ Performance Optimized: Built-in caching and performance monitoring
✅ Health Checks: Monitor system health and database connectivity
✅ Production Ready: Comprehensive logging, error handling, and metrics
Quick Start
1. Installation
bashdotnet add package DatabaseRulesEngine
2. Setup (ASP.NET Core)
csharp// Program.cs
builder.Services.AddRulesEngine(options =>
    options.UseSqlServer(connectionString));

// Add optional services
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
3. Basic Usage
csharp// Define your domain model
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal TotalPurchases { get; set; }
    public bool IsVip { get; set; }
    
    public void UpgradeToVip()
    {
        IsVip = true;
        Console.WriteLine($"{Name} upgraded to VIP!");
    }
}

// Use the rules engine
public class CustomerService
{
    private readonly IRulesEngine _rulesEngine;
    
    public CustomerService(IRulesEngine rulesEngine)
    {
        _rulesEngine = rulesEngine;
    }
    
    public async Task ProcessCustomer(Customer customer)
    {
        var result = await _rulesEngine.EvaluateAsync(customer);
        
        Console.WriteLine($"Matched {result.MatchedRules.Count} rules");
        Console.WriteLine($"Executed {result.ExecutedActions.Count} actions");
    }
}
4. Create Rules Dynamically
csharp// Create a VIP upgrade rule
var rule = new Rule
{
    Name = "VIP Customer Upgrade",
    Description = "Upgrade customers with high purchase amounts",
    TargetType = "Customer",
    Priority = 10
};

// Add conditions
rule.Conditions.Add(new RuleCondition
{
    PropertyName = "TotalPurchases",
    Operator = "GreaterThanOrEqual", 
    Value = "1000",
    SequenceOrder = 1
});

rule.Conditions.Add(new RuleCondition
{
    PropertyName = "IsVip",
    Operator = "Equals",
    Value = "False",
    LogicalOperator = "AND",
    SequenceOrder = 2
});

// Add actions
rule.Actions.Add(new RuleAction
{
    ActionType = "SetProperty",
    Target = "IsVip",
    Parameters = "True",
    ExecutionOrder = 1
});

rule.Actions.Add(new RuleAction
{
    ActionType = "InvokeMethod", 
    Target = "UpgradeToVip",
    ExecutionOrder = 2
});

// Save the rule
await rulesEngine.CreateRuleAsync(rule);
Advanced Features
Rule Builder Pattern
csharpvar rule = new RuleBuilder("Senior Discount", "Customer")
    .WithDescription("Apply discount for senior customers")
    .WithPriority(7)
    .AddCondition("Age", "GreaterThanOrEqual", "65")
    .AddCondition("MembershipLevel", "NotEquals", "VIP", "AND")
    .AddAction("SetProperty", "HasSeniorDiscount", "True")
    .AddAction("LogMessage", "", "Senior discount applied")
    .Build();
Custom Actions
csharppublic class CustomActionHandler : ICustomActionHandler
{
    public async Task ExecuteAsync(string actionName, object targetObject, string parameters)
    {
        switch (actionName.ToLower())
        {
            case "sendemail":
                await HandleSendEmail(targetObject, parameters);
                break;
            case "triggerworkflow":
                await HandleTriggerWorkflow(targetObject, parameters);
                break;
        }
    }
}

// Register custom handler
builder.Services.AddScoped<ICustomActionHandler, CustomActionHandler>();
Performance Monitoring
csharppublic class CustomerService
{
    private readonly RulePerformanceMonitor _monitor;
    
    public async Task ProcessWithMonitoring(Customer customer)
    {
        var result = await _monitor.MonitorRuleExecution(
            obj => _rulesEngine.EvaluateAsync(obj),
            customer);
    }
}
Caching
csharp// Rules are automatically cached for better performance
// Cache can be invalidated when rules change
await _ruleCacheService.InvalidateCacheAsync("Customer");
Supported Operators

Equals / NotEquals
GreaterThan / LessThan / GreaterThanOrEqual / LessThanOrEqual
Contains / NotContains
StartsWith / EndsWith
IsNull / IsNotNull
In / NotIn (comma-separated values)

Supported Actions

SetProperty: Set object property values
InvokeMethod: Call object methods
SendNotification: Send emails, SMS, or push notifications
LogMessage: Write log messages
Custom: Execute custom action handlers

Database Schema
The rules engine creates three main tables:

Rules: Main rule definitions
RuleConditions: Rule evaluation conditions
RuleActions: Actions to execute when rules match

Configuration
csharpbuilder.Services.AddRulesEngine(dbOptions => 
    dbOptions.UseSqlServer(connectionString),
    ruleOptions => {
        ruleOptions.CacheExpirationMinutes = 15;
        ruleOptions.EnablePerformanceLogging = true;
        ruleOptions.MaxRuleExecutionTimeSeconds = 30;
    });
Health Checks
csharpbuilder.Services.AddHealthChecks()
    .AddCheck<RulesEngineHealthCheck>("rules-engine");
Migration Support
bash# Add Entity Framework migrations
dotnet ef migrations add InitialRulesEngine
dotnet ef database update
Examples
Check out the Examples folder for:

E-commerce order processing
Customer lifecycle management
Pricing and discount rules
Compliance and validation rules
Workflow automation

API Documentation
For detailed API documentation, visit our Wiki or check the XML documentation included in the package.
Contributing
We welcome contributions! Please see our Contributing Guidelines for details.
License
This project is licensed under the MIT License - see the LICENSE file for details.
Support

📖 Documentation
🐛 Issue Tracker
💬 Discussions

Changelog
v1.0.0 (Initial Release)

Database-driven rule management
Generic object evaluation
Extensible action system
Performance monitoring and caching
Health checks integration
Comprehensive documentation