using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
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

    private object ConvertValue ( List<ActionParameter> parameters, Type targetType )
    {
        if (parameters == null || !parameters.Any())
            return null;

        try
        {
            // If the target expects a simple type, just take the first parameter
            if (targetType == typeof(string))
                return parameters.First().Value;

            if (targetType == typeof(int))
                return int.Parse(parameters.First().Value);

            if (targetType == typeof(decimal))
                return decimal.Parse(parameters.First().Value);

            if (targetType == typeof(double))
                return double.Parse(parameters.First().Value);

            if (targetType == typeof(DateTime))
                return DateTime.Parse(parameters.First().Value);

            if (targetType == typeof(bool))
                return bool.Parse(parameters.First().Value);

            // If target type is an object or complex DTO, 
            // map parameters into a dictionary and then serialize/deserialize
            if (!targetType.IsPrimitive && targetType != typeof(string))
            {
                var dict = parameters.ToDictionary(p => p.ParameterName, p => p.Value);

                // Newtonsoft.Json is more forgiving than System.Text.Json
                var json = JsonConvert.SerializeObject(dict);
                return JsonConvert.DeserializeObject(json, targetType);
            }

            // fallback
            return Convert.ChangeType(parameters.First().Value, targetType);
        }
        catch
        {
            return null;
        }
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

    public async Task<List<Rule>> GetActiveRulesAsync ()
    {
        return await _context.Rules
            .Include(r => r.Conditions)
            .Include(r => r.Actions)
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

    public async Task<InputContext> ExecuteRulesAsync ( InputContext context )
    {
        var rules = await GetActiveRulesAsync();

        foreach (var rule in rules.OrderBy(r => r.Priority))
        {
            if (!rule.IsActive)
                continue;

            // Evaluate conditions (using your static ConditionEvaluator)
            if (rule.Conditions.All(c => ConditionEvaluator.Evaluate(c, context)))
            {
                foreach (var actionDef in rule.Actions.OrderBy(a => a.ExecutionOrder))
                {
                    try
                    {
                        await ExecuteActionAsync(context, actionDef);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing action {actionDef.ActionType}: {ex.Message}");
                    }
                }
            }
        }

        return context;
    }

}