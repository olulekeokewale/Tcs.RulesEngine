using Tcs.BusinessRulesEngine.Rules;

public class RuleBuilder
{
    private readonly Rule _rule;

    public RuleBuilder ( string name, string targetType )
    {
        _rule = new Rule
        {
            Name = name,
            TargetType = targetType,
            IsActive = true,
            Conditions = new List<RuleCondition>(),
            Actions = new List<RuleAction>()
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

    public RuleBuilder AddAction ( string actionType, string target, Dictionary<string, string> parameters = null )
    {
        var action = new RuleAction
        {
            ActionType = actionType,
            Target = target,
            ExecutionOrder = _rule.Actions.Count + 1
        };

        if (parameters != null)
        {
            foreach (var kv in parameters)
            {
                action.Parameters.Add(new ActionParameter
                {
                    ParameterName = kv.Key,
                    Value = kv.Value
                });
            }
        }

        _rule.Actions.Add(action);
        return this;
    }

    public Rule Build () => _rule;
}
