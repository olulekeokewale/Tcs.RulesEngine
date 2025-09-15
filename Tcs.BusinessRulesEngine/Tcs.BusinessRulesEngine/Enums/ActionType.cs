using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcs.BusinessRulesEngine.Enums;
public enum ActionType
{
    SetProperty,
    InvokeMethod,
    SendNotification,
    LogMessage,
    Custom
}