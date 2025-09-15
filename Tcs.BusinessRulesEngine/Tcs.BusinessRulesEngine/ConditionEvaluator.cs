using Tcs.BusinessRulesEngine.Rules;
using Tcs.BusinessRulesEngine;
using Microsoft.EntityFrameworkCore;
namespace Tcs.BusinessRulesEngine;
public class ConditionEvaluator
{
  
    public static bool Evaluate<T> ( RuleCondition condition, T targetObject )
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

  
    public static async Task<RuleEvaluationResult> EvaluateAsync<T> ( T targetObject )
    {
        return await EvaluateAsync(targetObject, null);
    }

    public static async Task<RuleEvaluationResult> EvaluateAsync<T> ( T targetObject,   IList<Rule> rules )
    {
        var result = new RuleEvaluationResult { ModifiedObject = targetObject };

        try
        {
            //var typeName = typeof(T).Name;
            //var query = _context.Rules
            //    .Include(r => r.Conditions)
            //    .Include(r => r.Actions)
            //    .Where(r => r.IsActive && r.TargetType == typeName);

            //if (!string.IsNullOrEmpty(ruleFilter))
            //{
            //    query = query.Where(r => r.Name.Contains(ruleFilter) || r.Description.Contains(ruleFilter));
            //}

            //var rules = await query.OrderByDescending(r => r.Priority).ToListAsync();

            foreach (var rule in rules)
            {
                if (await EvaluateConditionsAsync(targetObject, rule.Conditions))
                {
                    result.MatchedRules.Add(rule);
                  //  await ConditionEvaluator.ExecuteActionsAsync(targetObject, rule.Actions, result);
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

    private static async Task<bool> EvaluateConditionsAsync<T> ( T targetObject, ICollection<RuleCondition> conditions )
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

    private static bool EvaluateCondition<T> ( T targetObject, RuleCondition condition )
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

    private static bool EvaluateBooleanExpression ( List<bool> results, List<string> operators )
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

    private static object ConvertValue ( string value, Type targetType )
    {
        if (value == null) return null;

        try
        {
            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(int))
                return int.Parse(value);

            if (targetType == typeof(decimal))
                return decimal.Parse(value);

            if (targetType == typeof(double))
                return double.Parse(value);

            if (targetType == typeof(DateTime))
                return DateTime.Parse(value);

            if (targetType == typeof(bool))
                return bool.Parse(value);

            // Fallback to Convert.ChangeType for other types
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return null;
        }
    }

  
}
