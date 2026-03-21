using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Domain.Enums;
using FlexiRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Infrastructure.Services;

public interface IAdminService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<PagedResult<AdminUserDto>> GetUsersAsync(int pageSize, Guid? cursor);
    Task<AdminUserDto> UpdateUserAsync(Guid userId, AdminUpdateUserDto dto);
    Task DeleteUserAsync(Guid userId);
    Task ApproveVendorAsync(Guid userId);
    Task ApproveServiceProviderAsync(Guid userId);
    Task<PagedResult<AdminReviewDto>> GetReviewsAsync(int pageSize, Guid? cursor);
    Task UpdateReviewAsync(Guid reviewId, bool isHidden);
    Task<PagedResult<AdminVerificationDto>> GetVerificationsAsync(int pageSize, Guid? cursor);
    Task UpdateVerificationAsync(Guid userId, AdminVerificationUpdateDto dto);
    Task<List<CurrencyDto>> GetCurrenciesAsync();
    Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto dto);
    Task<CurrencyDto> UpdateCurrencyAsync(Guid id, CreateCurrencyDto dto);
    Task DeleteCurrencyAsync(Guid id);
    Task<FinancialReportDto> GetFinancialReportAsync(DateTime? from, DateTime? to);
    Task<AnalyticsDto> GetAnalyticsAsync();
    Task OverrideFlexiScoreAsync(Guid userId, FlexiScoreOverrideDto dto);
}

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public AdminService(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalUsers = await _db.Users.CountAsync();
        var totalProperties = await _db.Properties.CountAsync();
        var totalBookings = await _db.Bookings.CountAsync();
        var pendingApprovals = await _db.Properties
            .CountAsync(p => p.Status == PropertyStatus.Pending);
        var totalRevenue = await _db.RentalPayments
            .Where(p => p.Status == PaymentStatus.Successful)
            .SumAsync(p => p.Amount);
        var activeLeases = await _db.RentalLeases
            .CountAsync(l => l.Status == LeaseStatus.Active);
        var overduePayments = await _db.PaymentSchedules
            .CountAsync(s => s.Status == PaymentScheduleStatus.Overdue);

        var bookingsByStatus = await _db.Bookings
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var propertiesByType = await _db.Properties
            .GroupBy(p => p.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        return new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            TotalProperties = totalProperties,
            TotalBookings = totalBookings,
            PendingApprovals = pendingApprovals,
            TotalRevenue = totalRevenue,
            ActiveLeases = activeLeases,
            OverduePayments = overduePayments,
            BookingsByStatus = bookingsByStatus,
            PropertiesByType = propertiesByType
        };
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(int pageSize, Guid? cursor)
    {
        var query = _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Roles)
            .OrderByDescending(u => u.CreatedAt)
            .AsQueryable();

        if (cursor.HasValue)
        {
            var cursorDate = await _db.Users
                .Where(u => u.Id == cursor.Value)
                .Select(u => u.CreatedAt)
                .FirstOrDefaultAsync();

            if (cursorDate != default)
                query = query.Where(u => u.CreatedAt < cursorDate);
        }

        var total = await query.CountAsync();
        var items = await query.Take(pageSize + 1).ToListAsync();
        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return new PagedResult<AdminUserDto>
        {
            Items = items.Select(MapUserToDto).ToList(),
            NextCursor = hasMore ? items.Last().Id : null,
            HasMore = hasMore,
            TotalCount = total
        };
    }

    public async Task<AdminUserDto> UpdateUserAsync(Guid userId, AdminUpdateUserDto dto)
    {
        var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new ApplicationException("User not found.");

        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        // Update roles
        _db.UserRoles.RemoveRange(user.Roles);
        foreach (var role in dto.Roles)
        {
            _db.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = role,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return MapUserToDto(user);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new ApplicationException("User not found.");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ApproveVendorAsync(Guid userId)
    {
        await AssignRoleAsync(userId, AppRole.Vendor);
        await _notificationService.SendAsync(
            userId,
            NotificationType.VerificationApproved,
            "Vendor Application Approved",
            "Your vendor application has been approved. You can now list products.");
    }

    public async Task ApproveServiceProviderAsync(Guid userId)
    {
        await AssignRoleAsync(userId, AppRole.ServiceProvider);
        await _notificationService.SendAsync(
            userId,
            NotificationType.VerificationApproved,
            "Service Provider Application Approved",
            "Your service provider application has been approved.");
    }

    public async Task<PagedResult<AdminReviewDto>> GetReviewsAsync(int pageSize, Guid? cursor)
    {
        var query = _db.Reviews
            .OrderByDescending(r => r.CreatedAt)
            .AsQueryable();

        if (cursor.HasValue)
        {
            var cursorDate = await _db.Reviews
                .Where(r => r.Id == cursor.Value)
                .Select(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (cursorDate != default)
                query = query.Where(r => r.CreatedAt < cursorDate);
        }

        var total = await query.CountAsync();
        var items = await query.Take(pageSize + 1).ToListAsync();
        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return new PagedResult<AdminReviewDto>
        {
            Items = items.Select(r => new AdminReviewDto
            {
                Id = r.Id,
                TargetId = r.TargetId,
                TargetType = r.TargetType,
                Rating = r.Rating,
                Comment = r.Comment,
                IsHidden = r.IsHidden,
                CreatedAt = r.CreatedAt
            }).ToList(),
            NextCursor = hasMore ? items.Last().Id : null,
            HasMore = hasMore,
            TotalCount = total
        };
    }

    public async Task UpdateReviewAsync(Guid reviewId, bool isHidden)
    {
        var review = await _db.Reviews.FindAsync(reviewId)
            ?? throw new ApplicationException("Review not found.");

        review.IsHidden = isHidden;
        review.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<AdminVerificationDto>> GetVerificationsAsync(
        int pageSize, Guid? cursor)
    {
        var query = _db.Profiles
            .Include(p => p.User)
            .Where(p => p.VerificationDocumentUrl != null)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        if (cursor.HasValue)
        {
            var cursorDate = await _db.Profiles
                .Where(p => p.UserId == cursor.Value)
                .Select(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (cursorDate != default)
                query = query.Where(p => p.CreatedAt < cursorDate);
        }

        var total = await query.CountAsync();
        var items = await query.Take(pageSize + 1).ToListAsync();
        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return new PagedResult<AdminVerificationDto>
        {
            Items = items.Select(p => new AdminVerificationDto
            {
                UserId = p.UserId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.User.Email,
                DocumentUrl = p.VerificationDocumentUrl,
                IsVerified = p.IsVerified,
                VerifiedAt = p.VerifiedAt
            }).ToList(),
            NextCursor = hasMore ? items.Last().UserId : null,
            HasMore = hasMore,
            TotalCount = total
        };
    }

    public async Task UpdateVerificationAsync(Guid userId, AdminVerificationUpdateDto dto)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ApplicationException("Profile not found.");

        profile.IsVerified = dto.Approve;
        profile.VerifiedAt = dto.Approve ? DateTime.UtcNow : null;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _notificationService.SendAsync(
            userId,
            dto.Approve
                ? NotificationType.VerificationApproved
                : NotificationType.VerificationRejected,
            dto.Approve ? "Verification Approved" : "Verification Rejected",
            dto.Approve
                ? "Your identity has been verified successfully."
                : $"Your verification was rejected. {dto.Reason}");
    }

    public async Task<List<CurrencyDto>> GetCurrenciesAsync()
    {
        return await _db.Currencies
            .OrderBy(c => c.Code)
            .Select(c => new CurrencyDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Symbol = c.Symbol,
                IsActive = c.IsActive
            })
            .ToListAsync();
    }

    public async Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto dto)
    {
        var currency = new Currency
        {
            Id = Guid.NewGuid(),
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            Symbol = dto.Symbol,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Currencies.Add(currency);
        await _db.SaveChangesAsync();

        return new CurrencyDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol,
            IsActive = currency.IsActive
        };
    }

    public async Task<CurrencyDto> UpdateCurrencyAsync(Guid id, CreateCurrencyDto dto)
    {
        var currency = await _db.Currencies.FindAsync(id)
            ?? throw new ApplicationException("Currency not found.");

        currency.Code = dto.Code.ToUpper();
        currency.Name = dto.Name;
        currency.Symbol = dto.Symbol;

        await _db.SaveChangesAsync();

        return new CurrencyDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol,
            IsActive = currency.IsActive
        };
    }

    public async Task DeleteCurrencyAsync(Guid id)
    {
        var currency = await _db.Currencies.FindAsync(id)
            ?? throw new ApplicationException("Currency not found.");

        _db.Currencies.Remove(currency);
        await _db.SaveChangesAsync();
    }

    public async Task<FinancialReportDto> GetFinancialReportAsync(
        DateTime? from, DateTime? to)
    {
        var query = _db.RentalPayments.AsQueryable();

        if (from.HasValue) query = query.Where(p => p.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(p => p.CreatedAt <= to.Value);

        var totalRevenue = await query
            .Where(p => p.Status == PaymentStatus.Successful)
            .SumAsync(p => p.Amount);

        var totalPending = await query
            .Where(p => p.Status == PaymentStatus.Pending)
            .SumAsync(p => p.Amount);

        var totalFailed = await query
            .Where(p => p.Status == PaymentStatus.Failed)
            .SumAsync(p => p.Amount);

        var totalTransactions = await query.CountAsync();

        var monthlyBreakdown = await query
            .Where(p => p.Status == PaymentStatus.Successful)
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(p => p.Amount),
                Transactions = g.Count()
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToListAsync();

        return new FinancialReportDto
        {
            TotalRevenue = totalRevenue,
            TotalPending = totalPending,
            TotalFailed = totalFailed,
            TotalTransactions = totalTransactions,
            MonthlyBreakdown = monthlyBreakdown
        };
    }

    public async Task<AnalyticsDto> GetAnalyticsAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var newUsersThisMonth = await _db.Users
            .CountAsync(u => u.CreatedAt >= monthStart);

        var newPropertiesThisMonth = await _db.Properties
            .CountAsync(p => p.CreatedAt >= monthStart);

        var bookingsThisMonth = await _db.Bookings
            .CountAsync(b => b.CreatedAt >= monthStart);

        var revenueThisMonth = await _db.RentalPayments
            .Where(p => p.Status == PaymentStatus.Successful
                && p.CreatedAt >= monthStart)
            .SumAsync(p => p.Amount);

        var topRegions = await _db.Properties
            .GroupBy(p => p.Region)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToListAsync();

        var usersByRole = await _db.UserRoles
            .GroupBy(r => r.Role)
            .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Role, x => x.Count);

        return new AnalyticsDto
        {
            NewUsersThisMonth = newUsersThisMonth,
            NewPropertiesThisMonth = newPropertiesThisMonth,
            BookingsThisMonth = bookingsThisMonth,
            RevenueThisMonth = revenueThisMonth,
            TopRegions = topRegions,
            UsersByRole = usersByRole
        };
    }

    public async Task OverrideFlexiScoreAsync(Guid userId, FlexiScoreOverrideDto dto)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new ApplicationException("User not found.");

        // FlexiScore stored on profile — add field if not present
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ApplicationException("Profile not found.");

        profile.FlexiScore = dto.Score;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task AssignRoleAsync(Guid userId, AppRole role)
    {
        var exists = await _db.UserRoles
            .AnyAsync(r => r.UserId == userId && r.Role == role);

        if (!exists)
        {
            _db.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = role,
                AssignedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }

    private static AdminUserDto MapUserToDto(User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        FirstName = u.Profile?.FirstName ?? string.Empty,
        LastName = u.Profile?.LastName ?? string.Empty,
        IsActive = u.IsActive,
        IsEmailVerified = u.EmailConfirmed,
        IsVerified = u.Profile?.IsVerified ?? false,
        Roles = u.Roles.Select(r => r.Role.ToString()).ToList(),
        CreatedAt = u.CreatedAt
    };
}