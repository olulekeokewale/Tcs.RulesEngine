using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcs.BusinessRulesEngine.Rules;


public class RuleAction
{
    [Key]
    public int Id { get; set; }

    public int RuleId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } // SetProperty, InvokeMethod, SendNotification, etc.

    [MaxLength(100)]
    public string Target { get; set; } // Property name or method name

    [MaxLength(500)]
    public string Parameters { get; set; } // JSON string for parameters

    public int ExecutionOrder { get; set; }

    // Navigation property
    [ForeignKey("RuleId")]
    public virtual Rule Rule { get; set; }
}