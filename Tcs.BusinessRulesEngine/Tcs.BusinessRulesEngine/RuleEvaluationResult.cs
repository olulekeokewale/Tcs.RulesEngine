using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tcs.BusinessRulesEngine.Rules;

namespace Tcs.BusinessRulesEngine;
public class RuleEvaluationResult
{
    public bool Success { get; set; }
    public List<Rule> MatchedRules { get; set; } = new List<Rule>();
    public List<string> ExecutedActions { get; set; } = new List<string>();
    public List<string> Errors { get; set; } = new List<string>();
    public object ModifiedObject { get; set; }
}