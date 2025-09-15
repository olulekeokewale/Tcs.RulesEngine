using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tcs.BusinessRulesEngine.Rules;

namespace Tcs.BusinessRulesEngine;

public interface IRuleAction
{
    string Name { get; }   // e.g. "ApplyDiscount"
    Task ExecuteAsync ( ActionParameters parameters, InputContext context );
    Task ExecuteAsync ( string parameters, InputContext context );

}