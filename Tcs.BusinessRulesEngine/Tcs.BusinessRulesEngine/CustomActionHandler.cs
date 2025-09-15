using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcs.BusinessRulesEngine;
public class CustomActionHandler : ICustomActionHandler
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<CustomActionHandler> _logger;

    public CustomActionHandler ( INotificationService notificationService, ILogger<CustomActionHandler> logger )
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync ( string actionName, object targetObject, string parameters )
    {
        switch (actionName.ToLower())
        {
            case "sendemail":
                await HandleSendEmail(targetObject, parameters);
                break;

            case "updateauditlog":
                await HandleUpdateAuditLog(targetObject, parameters);
                break;

            case "triggerworkflow":
                await HandleTriggerWorkflow(targetObject, parameters);
                break;

            default:
                _logger.LogWarning($"Unknown custom action: {actionName}");
                break;
        }
    }

    private async Task HandleSendEmail ( object targetObject, string parameters )
    {
        // Parse parameters (could be JSON)
        var emailParams = System.Text.Json.JsonSerializer.Deserialize<EmailParameters>(parameters);
        await _notificationService.SendEmailAsync(emailParams.To, emailParams.Subject, emailParams.Body);
    }

    private async Task HandleUpdateAuditLog ( object targetObject, string parameters )
    {
        _logger.LogInformation($"Audit: Rule executed on {targetObject.GetType().Name} - {parameters}");
        await Task.CompletedTask;
    }

    private async Task HandleTriggerWorkflow ( object targetObject, string parameters )
    {
        // Trigger external workflow system
        _logger.LogInformation($"Triggering workflow: {parameters}");
        await Task.CompletedTask;
    }
}