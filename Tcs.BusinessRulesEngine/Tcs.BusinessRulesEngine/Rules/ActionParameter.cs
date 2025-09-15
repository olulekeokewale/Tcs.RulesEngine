using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcs.BusinessRulesEngine.Rules;

public class ActionParameter
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ParameterName { get; set; } // e.g. "Amount", "Message", etc.

    [Required]
    [MaxLength(500)]
    public string Value { get; set; }

    [MaxLength(50)]
    public string DataType { get; set; } // Optional: "int", "decimal", "string", etc.

    public int RuleActionId { get; set; }

    [ForeignKey("RuleActionId")]
    public virtual RuleAction RuleAction { get; set; }
}