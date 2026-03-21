using FlexiRent.Domain.Enums;
using FlexiRent.Infrastructure.Persistence;
using FlexiRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlexiRent.Infrastructure.Jobs;

public interface IPaymentSchedulerJob
{
    Task ProcessDuePaymentsAsync();
    Task MarkOverduePaymentsAsync();
}

public class PaymentSchedulerJob : IPaymentSchedulerJob
{
    private readonly AppDbContext _db;
    private readonly IPaystackService _paystack;
    private readonly ILogger<PaymentSchedulerJob> _logger;

    public PaymentSchedulerJob(
        AppDbContext db,
        IPaystackService paystack,
        ILogger<PaymentSchedulerJob> logger)
    {
        _db = db;
        _paystack = paystack;
        _logger = logger;
    }

    public async Task ProcessDuePaymentsAsync()
    {
        var today = DateTime.UtcNow.Date;

        var dueSchedules = await _db.PaymentSchedules
            .Include(s => s.Lease)
                .ThenInclude(l => l.Tenant)
            .Where(s =>
                s.DueDate.Date == today &&
                s.Status == PaymentScheduleStatus.Upcoming)
            .ToListAsync();

        _logger.LogInformation(
            "Processing {Count} due payment schedules for {Date}",
            dueSchedules.Count, today);

        foreach (var schedule in dueSchedules)
        {
            try
            {
                schedule.Status = PaymentScheduleStatus.Due;
                _logger.LogInformation(
                    "Marked schedule {ScheduleId} as due for lease {LeaseId}",
                    schedule.Id, schedule.LeaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process schedule {ScheduleId}", schedule.Id);
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task MarkOverduePaymentsAsync()
    {
        var today = DateTime.UtcNow.Date;

        var overdueSchedules = await _db.PaymentSchedules
            .Where(s =>
                s.DueDate.Date < today &&
                (s.Status == PaymentScheduleStatus.Upcoming ||
                 s.Status == PaymentScheduleStatus.Due))
            .ToListAsync();

        _logger.LogInformation(
            "Marking {Count} schedules as overdue", overdueSchedules.Count);

        foreach (var schedule in overdueSchedules)
        {
            schedule.Status = PaymentScheduleStatus.Overdue;
            schedule.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
}