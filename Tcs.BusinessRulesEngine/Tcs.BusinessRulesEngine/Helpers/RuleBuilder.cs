using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tcs.BusinessRulesEngine.Rules;

namespace Tcs.BusinessRulesEngine.Helpers;
// Additional helper class for complex rule scenarios
public class RuleBuilder
{
    private readonly Rule _rule;

    public RuleBuilder ( string name, string targetType )
    {
        _rule = new Rule
        {
            Name = name,
            TargetType = targetType,
            IsActive = true
        };
    }

    public RuleBuilder WithDescription ( string description )
    {
        _rule.Description = description;
        return this;
    }

    public RuleBuilder WithPriority ( int priority )
    {
        _rule.Priority = priority;
        return this;
    }

    public RuleBuilder AddCondition ( string property, string op, string value, string logicalOp = "AND" )
    {
        _rule.Conditions.Add(new RuleCondition
        {
            PropertyName = property,
            Operator = op,
            Value = value,
            LogicalOperator = logicalOp,
            SequenceOrder = _rule.Conditions.Count + 1
        });
        return this;
    }

    public RuleBuilder AddAction ( string actionType, string target, string parameters = null )
    {
        _rule.Actions.Add(new RuleAction
        {
            ActionType = actionType,
            Target = target,
            Parameters = parameters,
            ExecutionOrder = _rule.Actions.Count + 1
        });
        return this;
    }

    public Rule Build () => _rule;
}

// Usage example with RuleBuilder:
/*
var rule = new RuleBuilder("Senior Citizen Discount", "Customer")
    .WithDescription("Apply discount for senior citizens")
    .WithPriority(7)
    .AddCondition("Age", "GreaterThanOrEqual", "65")
    .AddCondition("MembershipLevel", "NotEquals", "VIP", "AND")
    .AddAction("SetProperty", "HasSeniorDiscount", "True")
    .AddAction("LogMessage", "", "Senior citizen discount applied")
    .Build();
*/
