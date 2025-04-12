using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Restaurant_Manager.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Restaurant_Manager.Services;

public class ReservationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ReservationCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var now = DateTime.Now;
                var expiredReservations = await context.Reservations
                    .Where(r => r.Status == "pending" && r.ReservationTime.AddMinutes(15) <= now)
                    .ToListAsync();

                foreach (var reservation in expiredReservations)
                {
                    reservation.Status = "cancelled";
                }

                if (expiredReservations.Any())
                {
                    await context.SaveChangesAsync();
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); 
        }
    }
}
