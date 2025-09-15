using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tcs.BusinessRulesEngine.Rules;

namespace Tcs.BusinessRulesEngine;
public class RulesEngine : IRulesEngine
{
    private readonly RulesEngineContext _context;
    private readonly IServiceProvider _serviceProvider;

    public RulesEngine ( RulesEngineContext context, IServiceProvider serviceProvider )
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public async Task<RuleEvaluationResult> EvaluateAsync<T> ( T targetObject )
    {
        return await EvaluateAsync(targetObject, null);
    }

    public async Task<RuleEvaluationResult> EvaluateAsync<T> ( T targetObject, string ruleFilter )
    {
        var result = new RuleEvaluationResult { ModifiedObject = targetObject };

        try
        {
            var typeName = typeof(T).Name;
            var query = _context.Rules
                .Include(r => r.Conditions)
                .Include(r => r.Actions)
                .Where(r => r.IsActive && r.TargetType == typeName);

            if (!string.IsNullOrEmpty(ruleFilter))
            {
                query = query.Where(r => r.Name.Contains(ruleFilter) || r.Description.Contains(ruleFilter));
            }

            var rules = await query.OrderByDescending(r => r.Priority).ToListAsync();

            foreach (var rule in rules)
            {
                if (await EvaluateConditionsAsync(targetObject, rule.Conditions))
                {
                    result.MatchedRules.Add(rule);
                    await ExecuteActionsAsync(targetObject, rule.Actions, result);
                }
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error during rule evaluation: {ex.Message}");
            result.Success = false;
        }

        return result;
    }

    private async Task<bool> EvaluateConditionsAsync<T> ( T targetObject, ICollection<RuleCondition> conditions )
    {
        if (!conditions.Any()) return true;

        var orderedConditions = conditions.OrderBy(c => c.SequenceOrder).ToList();
        var results = new List<bool>();
        var operators = new List<string>();

        foreach (var condition in orderedConditions)
        {
            var conditionResult = EvaluateCondition(targetObject, condition);
            results.Add(conditionResult);

            if (condition != orderedConditions.Last())
            {
                operators.Add(condition.LogicalOperator ?? "AND");
            }
        }

        // Evaluate the boolean expression
        return EvaluateBooleanExpression(results, operators);
    }

    private bool EvaluateCondition<T> ( T targetObject, RuleCondition condition )
    {
        var property = typeof(T).GetProperty(condition.PropertyName);
        if (property == null) return false;

        var propertyValue = property.GetValue(targetObject);
        var conditionValue = ConvertValue(condition.Value, property.PropertyType);

        return condition.Operator.ToUpper() switch
        {
            "EQUALS" => Equals(propertyValue, conditionValue),
            "NOTEQUALS" => !Equals(propertyValue, conditionValue),
            "GREATERTHAN" => Comparer<object>.Default.Compare(propertyValue, conditionValue) > 0,
            "LESSTHAN" => Comparer<object>.Default.Compare(propertyValue, conditionValue) < 0,
            "GREATERTHANOREQUAL" => Comparer<object>.Default.Compare(propertyValue, conditionValue) >= 0,
            "LESSTHANOREQUAL" => Comparer<object>.Default.Compare(propertyValue, conditionValue) <= 0,
            "CONTAINS" => propertyValue?.ToString()?.Contains(condition.Value) == true,
            "NOTCONTAINS" => propertyValue?.ToString()?.Contains(condition.Value) != true,
            "STARTSWITH" => propertyValue?.ToString()?.StartsWith(condition.Value) == true,
            "ENDSWITH" => propertyValue?.ToString()?.EndsWith(condition.Value) == true,
            "ISNULL" => propertyValue == null,
            "ISNOTNULL" => propertyValue != null,
            "IN" => condition.Value?.Split(',').Contains(propertyValue?.ToString()) == true,
            "NOTIN" => condition.Value?.Split(',').Contains(propertyValue?.ToString()) != true,
            _ => false
        };
    }

    private bool EvaluateBooleanExpression ( List<bool> results, List<string> operators )
    {
        if (results.Count == 1) return results[0];

        var result = results[0];
        for (int i = 0; i < operators.Count; i++)
        {
            if (operators[i].ToUpper() == "AND")
            {
                result = result && results[i + 1];
            }
            else if (operators[i].ToUpper() == "OR")
            {
                result = result || results[i + 1];
            }
        }

        return result;
    }

    private async Task ExecuteActionsAsync<T> ( T targetObject, ICollection<RuleAction> actions, RuleEvaluationResult result )
    {
        var orderedActions = actions.OrderBy(a => a.ExecutionOrder);

        foreach (var action in orderedActions)
        {
            try
            {
                await ExecuteActionAsync(targetObject, action);
                result.ExecutedActions.Add($"{action.ActionType}: {action.Target}");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error executing action {action.Id}: {ex.Message}");
            }
        }
    }

    private async Task ExecuteActionAsync<T> ( T targetObject, RuleAction action )
    {
        switch (action.ActionType.ToUpper())
        {
            case "SETPROPERTY":
                ExecuteSetPropertyAction(targetObject, action);
                break;

            case "INVOKEMETHOD":
                await ExecuteInvokeMethodAction(targetObject, action);
                break;

            case "SENDNOTIFICATION":
                await ExecuteSendNotificationAction(targetObject, action);
                break;

            case "LOGMESSAGE":
                ExecuteLogMessageAction(targetObject, action);
                break;

            case "CUSTOM":
                await ExecuteCustomAction(targetObject, action);
                break;
        }
    }

    private void ExecuteSetPropertyAction<T> ( T targetObject, RuleAction action )
    {
        var property = typeof(T).GetProperty(action.Target);
        if (property != null && property.CanWrite)
        {
            var value = ConvertValue(action.Parameters, property.PropertyType);
            property.SetValue(targetObject, value);
        }
    }

    private async Task ExecuteInvokeMethodAction<T> ( T targetObject, RuleAction action )
    {
        var method = typeof(T).GetMethod(action.Target);
        if (method != null)
        {
            var result = method.Invoke(targetObject, null);
            if (result is Task task)
            {
                await task;
            }
        }
    }

    private async Task ExecuteSendNotificationAction<T> ( T targetObject, RuleAction action )
    {
        // Implement notification logic here
        // This could integrate with email services, message queues, etc.
        await Task.CompletedTask;
    }

    private void ExecuteLogMessageAction<T> ( T targetObject, RuleAction action )
    {
        // Implement logging logic here
        Console.WriteLine($"Rule Action Log: {action.Parameters}");
    }

    private async Task ExecuteCustomAction<T> ( T targetObject, RuleAction action )
    {
        // Allow for custom action implementations
        // This could use dependency injection to resolve custom action handlers
        await Task.CompletedTask;
    }

    private object ConvertValue ( string value, Type targetType )
    {
        if (string.IsNullOrEmpty(value)) return null;

        if (targetType == typeof(string)) return value;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return underlyingType.Name switch
        {
            nameof(Int32) => int.Parse(value),
            nameof(Int64) => long.Parse(value),
            nameof(Double) => double.Parse(value),
            nameof(Decimal) => decimal.Parse(value),
            nameof(Boolean) => bool.Parse(value),
            nameof(DateTime) => DateTime.Parse(value),
            nameof(Guid) => Guid.Parse(value),
            _ => Convert.ChangeType(value, underlyingType)
        };
    }

    public async Task<List<Rule>> GetRulesForTypeAsync ( string typeName )
    {
        return await _context.Rules
            .Include(r => r.Conditions)
            .Include(r => r.Actions)
            .Where(r => r.TargetType == typeName)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();
    }

    public async Task<Rule> CreateRuleAsync ( Rule rule )
    {
        rule.CreatedDate = DateTime.UtcNow;
        _context.Rules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<Rule> UpdateRuleAsync ( Rule rule )
    {
        rule.ModifiedDate = DateTime.UtcNow;
        _context.Rules.Update(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task DeleteRuleAsync ( int ruleId )
    {
        var rule = await _context.Rules.FindAsync(ruleId);
        if (rule != null)
        {
            _context.Rules.Remove(rule);
            await _context.SaveChangesAsync();
        }
    }
}