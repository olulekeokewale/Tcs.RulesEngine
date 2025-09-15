using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcs.BusinessRulesEngine.Enums;
public enum ComparisonOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    IsNull,
    IsNotNull,
    In,
    NotIn
}