using System.Threading.Tasks;
using DeliveryMarket.DataAccess.Db;
using DeliveryMarket.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DeliveryMarket.Tests.Integration
{
    public class HostTest
    {
        private static async Task InitTestDatabase(IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var scopedServices = scope.ServiceProvider;
            var measurement = scopedServices.GetRequiredService<IMeasurementRepository>();
            var device = scopedServices.GetRequiredService<IDeviceRepository>();

            var factory = scopedServices.GetRequiredService<IDbContextFactory<WeatherDbContext>>();
            var db = factory.CreateDbContext();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            await ContextInitializer.InitializeDb(measurement, device);
        }
    }
}
