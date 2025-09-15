using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcs.BusinessRulesEngine.Rules;
public class ActionParameters : Dictionary<string, object>
{
    public T Get<T> ( string key )
    {
        if (TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        throw new KeyNotFoundException($"Parameter {key} not found.");
    }
}
