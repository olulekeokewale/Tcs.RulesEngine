using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tcs.BusinessRulesEngine.Rules;

namespace Tcs.BusinessRulesEngine;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRulesEngine ( this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder )
    {
        services.AddDbContext<RulesEngineContext>(optionsBuilder);
        services.AddScoped<IRulesEngine, RulesEngine>();
        return services;
    }
}