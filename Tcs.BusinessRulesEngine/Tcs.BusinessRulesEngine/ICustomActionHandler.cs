using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcs.BusinessRulesEngine
{
    internal interface ICustomActionHandler
    {
        Task ExecuteAsync ( string actionName, object targetObject, string parameters );
    }
}
