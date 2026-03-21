using FlexiRent.Domain.Enums;
using FlexiRent.Infrastructure.Persistence;
using FlexiRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlexiRent.Infrastructure.Jobs;

public interface INotificationJob
{
    Task SendPaymentRemindersAsync();
    Task SendPropertyMatchAlertsAsync();
    Task SendLeaseExpiryAlertsAsync();
}

public class NotificationJob : INotificationJob
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationJob> _logger;

    public NotificationJob(
        AppDbContext db,
        INotificationService notificationService,
        ILogger<NotificationJob> logger)
    {
        _db = db;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task SendPaymentRemindersAsync()
    {
        var reminderDays = new[] { 7, 3, 1 };
        var today = DateTime.UtcNow.Date;

        foreach (var days in reminderDays)
        {
            var targetDate = today.AddDays(days);

            var schedules = await _db.PaymentSchedules
                .Include(s => s.Lease)
                .Where(s =>
                    s.DueDate.Date == targetDate &&
                    s.Status == PaymentScheduleStatus.Upcoming)
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                await _notificationService.SendAsync(
                    schedule.Lease.TenantId,
                    NotificationType.PaymentDue,
                    "Payment Due Soon",
                    $"Your rent payment of {schedule.Amount} {schedule.Lease.Currency} " +
                    $"is due in {days} day{(days == 1 ? "" : "s")}.",
                    actionUrl: $"/payments?leaseId={schedule.LeaseId}",
                    referenceId: schedule.Id);
            }

            _logger.LogInformation(
                "Sent {Count} payment reminders for {Days} days ahead",
                schedules.Count, days);
        }
    }

    public async Task SendPropertyMatchAlertsAsync()
    {
        var recentProperties = await _db.Properties
            .Where(p =>
                p.Status == PropertyStatus.Approved &&
                p.CreatedAt >= DateTime.UtcNow.AddHours(-24))
            .ToListAsync();

        if (!recentProperties.Any()) return;

        var profiles = await _db.Profiles
            .Where(p => p.PropertyAlerts)
            .ToListAsync();

        foreach (var profile in profiles)
        {
            var matches = recentProperties.Where(p =>
                (string.IsNullOrEmpty(profile.PreferredRegion) ||
                 p.Region.ToLower().Contains(profile.PreferredRegion.ToLower())) &&
                (!profile.MinPrice.HasValue || p.PricePerMonth >= profile.MinPrice) &&
                (!profile.MaxPrice.HasValue || p.PricePerMonth <= profile.MaxPrice) &&
                (!profile.MinBedrooms.HasValue || p.Bedrooms >= profile.MinBedrooms)
            ).ToList();

            if (!matches.Any()) continue;

            await _notificationService.SendAsync(
                profile.UserId,
                NotificationType.PropertyMatch,
                "New Properties Match Your Preferences",
                $"{matches.Count} new propert{(matches.Count == 1 ? "y" : "ies")} " +
                $"match{(matches.Count == 1 ? "es" : "")} your search preferences.",
                actionUrl: "/properties/search");
        }

        _logger.LogInformation(
            "Sent property match alerts for {Count} new properties",
            recentProperties.Count);
    }

    public async Task SendLeaseExpiryAlertsAsync()
    {
        var alertDays = new[] { 30, 14, 7 };
        var today = DateTime.UtcNow.Date;

        foreach (var days in alertDays)
        {
            var targetDate = today.AddDays(days);

            var leases = await _db.RentalLeases
                .Where(l =>
                    l.EndDate.Date == targetDate &&
                    l.Status == LeaseStatus.Active)
                .ToListAsync();

            foreach (var lease in leases)
            {
                await _notificationService.SendAsync(
                    lease.TenantId,
                    NotificationType.LeaseExpiringSoon,
                    "Lease Expiring Soon",
                    $"Your lease expires in {days} day{(days == 1 ? "" : "s")}. " +
                    $"Consider renewing it.",
                    actionUrl: $"/leases/{lease.Id}/renew",
                    referenceId: lease.Id);
            }

            _logger.LogInformation(
                "Sent {Count} lease expiry alerts for {Days} days ahead",
                leases.Count, days);
        }
    }
}