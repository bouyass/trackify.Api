using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trackify.Api.Data;
using Trackify.Api.Services;

namespace Trackify.Jobs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false)
                          .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
                                           ?? throw new InvalidOperationException("Connection string not found");

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(connectionString));

                    services.AddHttpClient();
                    services.AddScoped<UpdateScanService>();
                })
                .Build();

            using var scope = host.Services.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<UpdateScanService>();
            await job.RunDailyAsync();

            Console.WriteLine("✅ UpdateScan terminé.");
        }
    }
}
