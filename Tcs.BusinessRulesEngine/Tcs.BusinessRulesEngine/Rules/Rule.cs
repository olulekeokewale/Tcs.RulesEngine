

namespace Tcs.BusinessRulesEngine.Rules;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// Domain Models
public class Rule
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string TargetType { get; set; } // The type this rule applies to

    public bool IsActive { get; set; } = true;

    public int Priority { get; set; } = 0; // Higher number = higher priority

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    // Navigation properties
    public virtual ICollection<RuleCondition> Conditions { get; set; } = new List<RuleCondition>();
    public virtual ICollection<RuleAction> Actions { get; set; } = new List<RuleAction>();
}