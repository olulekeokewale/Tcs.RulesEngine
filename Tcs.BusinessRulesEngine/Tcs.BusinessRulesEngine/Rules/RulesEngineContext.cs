using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Tcs.BusinessRulesEngine.Rules;
public class RulesEngineContext : DbContext
{
    public RulesEngineContext ( DbContextOptions<RulesEngineContext> options ) : base(options) { }

    public DbSet<Rule> Rules { get; set; }
    public DbSet<RuleCondition> RuleConditions { get; set; }
    public DbSet<RuleAction> RuleActions { get; set; }

    protected override void OnModelCreating ( ModelBuilder modelBuilder )
    {
        modelBuilder.Entity<Rule>()
            .HasMany(r => r.Conditions)
            .WithOne(c => c.Rule)
            .HasForeignKey(c => c.RuleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rule>()
            .HasMany(r => r.Actions)
            .WithOne(a => a.Rule)
            .HasForeignKey(a => a.RuleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RuleCondition>()
            .HasIndex(c => new { c.RuleId, c.SequenceOrder });

        modelBuilder.Entity<RuleAction>()
            .HasIndex(a => new { a.RuleId, a.ExecutionOrder });
    }
}
