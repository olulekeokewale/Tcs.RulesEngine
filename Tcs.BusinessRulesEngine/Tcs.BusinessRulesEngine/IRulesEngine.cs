using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tcs.BusinessRulesEngine.Rules;

namespace Tcs.BusinessRulesEngine;
public interface IRulesEngine
{
    Task<RuleEvaluationResult> EvaluateAsync<T> ( T targetObject );
    Task<RuleEvaluationResult> EvaluateAsync<T> ( T targetObject, string ruleFilter );
    Task<List<Rule>> GetRulesForTypeAsync ( string typeName );
    Task<Rule> CreateRuleAsync ( Rule rule );
    Task<Rule> UpdateRuleAsync ( Rule rule );
    Task DeleteRuleAsync ( int ruleId );
}
