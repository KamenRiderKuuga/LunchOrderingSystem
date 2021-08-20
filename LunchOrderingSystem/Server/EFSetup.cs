using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace LunchOrderingSystem.Server
{
    public static class EFSetup
    {
        public static IHost SeedData(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MenuDbContext>();
                dbContext.Database.EnsureCreated();
            }

            return host;
        }
    }
}
