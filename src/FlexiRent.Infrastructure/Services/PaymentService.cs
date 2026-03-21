using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Domain.Enums;
using FlexiRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FlexiRent.Infrastructure.Services;

public interface IPaymentService
{
    Task<InitiatePaymentResponseDto> InitiatePaymentAsync(Guid userId, InitiatePaymentDto dto);
    Task<PaymentDto> VerifyPaymentAsync(string reference);
    Task HandleWebhookAsync(string payload, string signature);
    Task<RentalLeaseDto> CreateLeaseAsync(Guid userId, CreateLeaseDto dto);
    Task<RentalLeaseDto> RenewLeaseAsync(Guid leaseId, Guid userId);
    Task<PagedResult<PaymentDto>> GetPaymentHistoryAsync(Guid userId, int pageSize, Guid? cursor);
    Task<byte[]> ExportPaymentHistoryCsvAsync(Guid userId);
    Task<PaymentAccountDto> AddPaymentAccountAsync(Guid userId, CreatePaymentAccountDto dto);
    Task<List<PaymentAccountDto>> GetPaymentAccountsAsync(Guid userId);
    Task DeletePaymentAccountAsync(Guid accountId, Guid userId);
    Task ApprovePaymentAsync(Guid paymentId);
}

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly IPaystackService _paystack;
    private readonly IConfiguration _config;

    public PaymentService(AppDbContext db, IPaystackService paystack, IConfiguration config)
    {
        _db = db;
        _paystack = paystack;
        _config = config;
    }

    public async Task<InitiatePaymentResponseDto> InitiatePaymentAsync(
        Guid userId, InitiatePaymentDto dto)
    {
        var lease = await _db.RentalLeases
            .Include(l => l.Tenant)
            .FirstOrDefaultAsync(l => l.Id == dto.LeaseId && l.TenantId == userId)
            ?? throw new ApplicationException("Lease not found.");

        var reference = $"FLX-{Guid.NewGuid():N}".ToUpper();
        var callbackUrl = _config["Paystack:CallbackUrl"]
            ?? "https://flexirent.com/payments/callback";

        var paystackResponse = await _paystack.InitializeTransactionAsync(
            lease.Tenant.Email,
            dto.Amount,
            reference,
            callbackUrl);

        var payment = new RentalPayment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LeaseId = dto.LeaseId,
            ScheduleId = dto.ScheduleId,
            Amount = dto.Amount,
            Currency = lease.Currency,
            Status = PaymentStatus.Pending,
            PaystackReference = reference,
            CreatedAt = DateTime.UtcNow
        };

        _db.RentalPayments.Add(payment);
        await _db.SaveChangesAsync();

        return new InitiatePaymentResponseDto
        {
            PaymentId = payment.Id,
            Reference = reference,
            AuthorizationUrl = paystackResponse.AuthorizationUrl,
            AccessCode = paystackResponse.AccessCode
        };
    }

    public async Task<PaymentDto> VerifyPaymentAsync(string reference)
    {
        var payment = await _db.RentalPayments
            .FirstOrDefaultAsync(p => p.PaystackReference == reference)
            ?? throw new ApplicationException("Payment not found.");

        var verifyResponse = await _paystack.VerifyTransactionAsync(reference);

        payment.Status = verifyResponse.TransactionStatus == "success"
            ? PaymentStatus.Successful
            : PaymentStatus.Failed;

        payment.PaystackTransactionId = verifyResponse.TransactionId;
        payment.PaidAt = verifyResponse.TransactionStatus == "success"
            ? DateTime.UtcNow : null;
        payment.UpdatedAt = DateTime.UtcNow;

        if (payment.Status == PaymentStatus.Successful && payment.ScheduleId.HasValue)
        {
            var schedule = await _db.PaymentSchedules
                .FindAsync(payment.ScheduleId);
            if (schedule is not null)
            {
                schedule.Status = PaymentScheduleStatus.Paid;
                schedule.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return MapToDto(payment);
    }

    public async Task HandleWebhookAsync(string payload, string signature)
    {
        if (!_paystack.VerifyWebhookSignature(payload, signature))
            throw new ApplicationException("Invalid webhook signature.");

        var doc = System.Text.Json.JsonDocument.Parse(payload);
        var eventType = doc.RootElement.GetProperty("event").GetString();

        if (eventType != "charge.success") return;

        var reference = doc.RootElement
            .GetProperty("data")
            .GetProperty("reference")
            .GetString();

        if (string.IsNullOrEmpty(reference)) return;

        var payment = await _db.RentalPayments
            .FirstOrDefaultAsync(p => p.PaystackReference == reference);

        if (payment is null || payment.Status == PaymentStatus.Successful) return;

        payment.Status = PaymentStatus.Successful;
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        if (payment.ScheduleId.HasValue)
        {
            var schedule = await _db.PaymentSchedules.FindAsync(payment.ScheduleId);
            if (schedule is not null)
            {
                schedule.Status = PaymentScheduleStatus.Paid;
                schedule.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<RentalLeaseDto> CreateLeaseAsync(Guid userId, CreateLeaseDto dto)
    {
        var property = await _db.Properties
            .FirstOrDefaultAsync(p => p.Id == dto.PropertyId)
            ?? throw new ApplicationException("Property not found.");

        var lease = new RentalLease
        {
            Id = Guid.NewGuid(),
            TenantId = userId,
            PropertyId = dto.PropertyId,
            MonthlyAmount = dto.MonthlyAmount,
            Currency = dto.Currency,
            StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc),
            PaymentDayOfMonth = dto.PaymentDayOfMonth,
            Status = LeaseStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _db.RentalLeases.Add(lease);

        // Auto-generate payment schedule
        var schedules = GeneratePaymentSchedule(lease);
        _db.PaymentSchedules.AddRange(schedules);

        await _db.SaveChangesAsync();
        return MapLeaseToDto(lease);
    }

    public async Task<RentalLeaseDto> RenewLeaseAsync(Guid leaseId, Guid userId)
    {
        var lease = await _db.RentalLeases
            .FirstOrDefaultAsync(l => l.Id == leaseId && l.TenantId == userId)
            ?? throw new ApplicationException("Lease not found.");

        if (lease.Status != LeaseStatus.Active && lease.Status != LeaseStatus.Expired)
            throw new ApplicationException("Only active or expired leases can be renewed.");

        lease.Status = LeaseStatus.Renewed;

        var newLease = new RentalLease
        {
            Id = Guid.NewGuid(),
            TenantId = lease.TenantId,
            PropertyId = lease.PropertyId,
            MonthlyAmount = lease.MonthlyAmount,
            Currency = lease.Currency,
            StartDate = DateTime.SpecifyKind(lease.EndDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(
            lease.EndDate.AddMonths(
            (int)(lease.EndDate - lease.StartDate).TotalDays / 30),
            DateTimeKind.Utc),
            PaymentDayOfMonth = lease.PaymentDayOfMonth,
            Status = LeaseStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _db.RentalLeases.Add(newLease);
        _db.PaymentSchedules.AddRange(GeneratePaymentSchedule(newLease));

        await _db.SaveChangesAsync();
        return MapLeaseToDto(newLease);
    }

    public async Task<PagedResult<PaymentDto>> GetPaymentHistoryAsync(
        Guid userId, int pageSize, Guid? cursor)
    {
        var query = _db.RentalPayments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        if (cursor.HasValue)
        {
            var cursorDate = await _db.RentalPayments
                .Where(p => p.Id == cursor.Value)
                .Select(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (cursorDate != default)
                query = query.Where(p => p.CreatedAt < cursorDate);
        }

        var total = await query.CountAsync();
        var items = await query.Take(pageSize + 1).ToListAsync();
        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return new PagedResult<PaymentDto>
        {
            Items = items.Select(MapToDto).ToList(),
            NextCursor = hasMore ? items.Last().Id : null,
            HasMore = hasMore,
            TotalCount = total
        };
    }

    public async Task<byte[]> ExportPaymentHistoryCsvAsync(Guid userId)
    {
        var payments = await _db.RentalPayments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,Amount,Currency,Status,Reference,PaidAt,CreatedAt");

        foreach (var p in payments)
        {
            sb.AppendLine(
                $"{p.Id},{p.Amount},{p.Currency},{p.Status}," +
                $"{p.PaystackReference},{p.PaidAt:O},{p.CreatedAt:O}");
        }

        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<PaymentAccountDto> AddPaymentAccountAsync(
        Guid userId, CreatePaymentAccountDto dto)
    {
        if (dto.IsDefault)
        {
            var existing = await _db.PaymentAccounts
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();
            foreach (var a in existing)
                a.IsDefault = false;
        }

        var account = new PaymentAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = dto.Type,
            AccountName = dto.AccountName,
            AccountNumber = dto.AccountNumber,
            BankName = dto.BankName,
            MobileProvider = dto.MobileProvider,
            IsDefault = dto.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        _db.PaymentAccounts.Add(account);
        await _db.SaveChangesAsync();

        return MapAccountToDto(account);
    }

    public async Task<List<PaymentAccountDto>> GetPaymentAccountsAsync(Guid userId)
    {
        return await _db.PaymentAccounts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .Select(a => MapAccountToDto(a))
            .ToListAsync();
    }

    public async Task DeletePaymentAccountAsync(Guid accountId, Guid userId)
    {
        var account = await _db.PaymentAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new ApplicationException("Payment account not found.");

        _db.PaymentAccounts.Remove(account);
        await _db.SaveChangesAsync();
    }

    public async Task ApprovePaymentAsync(Guid paymentId)
    {
        var payment = await _db.RentalPayments
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new ApplicationException("Payment not found.");

        payment.IsAdminApproved = true;
        payment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<PaymentSchedule> GeneratePaymentSchedule(RentalLease lease)
    {
        var schedules = new List<PaymentSchedule>();
        var current = DateTime.SpecifyKind(
            new DateTime(
                lease.StartDate.Year,
                lease.StartDate.Month,
                Math.Min(lease.PaymentDayOfMonth,
                    DateTime.DaysInMonth(lease.StartDate.Year, lease.StartDate.Month))),
            DateTimeKind.Utc);

        var endDate = DateTime.SpecifyKind(lease.EndDate, DateTimeKind.Utc);

        while (current <= endDate)
        {
            schedules.Add(new PaymentSchedule
            {
                Id = Guid.NewGuid(),
                LeaseId = lease.Id,
                Amount = lease.MonthlyAmount,
                DueDate = current,
                Status = current < DateTime.UtcNow
                    ? PaymentScheduleStatus.Overdue
                    : PaymentScheduleStatus.Upcoming,
                CreatedAt = DateTime.UtcNow
            });
            current = current.AddMonths(1);
        }

        return schedules;
    }

    private static PaymentDto MapToDto(RentalPayment p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        LeaseId = p.LeaseId,
        Amount = p.Amount,
        Currency = p.Currency,
        Status = p.Status,
        Reference = p.PaystackReference,
        PaidAt = p.PaidAt,
        CreatedAt = p.CreatedAt
    };

    private static RentalLeaseDto MapLeaseToDto(RentalLease l) => new()
    {
        Id = l.Id,
        TenantId = l.TenantId,
        PropertyId = l.PropertyId,
        MonthlyAmount = l.MonthlyAmount,
        Currency = l.Currency,
        StartDate = l.StartDate,
        EndDate = l.EndDate,
        Status = l.Status,
        PaymentDayOfMonth = l.PaymentDayOfMonth,
        CreatedAt = l.CreatedAt
    };

    private static PaymentAccountDto MapAccountToDto(PaymentAccount a) => new()
    {
        Id = a.Id,
        Type = a.Type,
        AccountName = a.AccountName,
        AccountNumber = a.AccountNumber,
        BankName = a.BankName,
        MobileProvider = a.MobileProvider,
        IsDefault = a.IsDefault
    };
}