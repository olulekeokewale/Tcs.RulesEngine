using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcs.BusinessRulesEngine.Rules;

public class RuleCondition
{
    [Key]
    public int Id { get; set; }

    public int RuleId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PropertyName { get; set; }

    [Required]
    [MaxLength(20)]
    public string Operator { get; set; } // Equals, NotEquals, GreaterThan, LessThan, Contains, etc.

    [MaxLength(500)]
    public string Value { get; set; }

    [MaxLength(20)]
    public string LogicalOperator { get; set; } = "AND"; // AND, OR

    public int SequenceOrder { get; set; }

    // Navigation property
    [ForeignKey("RuleId")]
    public virtual Rule Rule { get; set; }
}