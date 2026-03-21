using FlexiRent.Domain.Enums;

namespace FlexiRent.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalProperties { get; set; }
    public int TotalBookings { get; set; }
    public int PendingApprovals { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveLeases { get; set; }
    public int OverduePayments { get; set; }
    public Dictionary<string, int> BookingsByStatus { get; set; } = new();
    public Dictionary<string, int> PropertiesByType { get; set; } = new();
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsVerified { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class AdminUpdateUserDto
{
    public bool IsActive { get; set; }
    public List<AppRole> Roles { get; set; } = new();
}

public class AdminReviewDto
{
    public Guid Id { get; set; }
    public Guid TargetId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminVerificationDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DocumentUrl { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class AdminVerificationUpdateDto
{
    public bool Approve { get; set; }
    public string? Reason { get; set; }
}

public class CurrencyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateCurrencyDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}

public class FinancialReportDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalFailed { get; set; }
    public int TotalTransactions { get; set; }
    public List<MonthlyRevenueDto> MonthlyBreakdown { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int Transactions { get; set; }
}

public class AnalyticsDto
{
    public int TotalVisits { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int NewPropertiesThisMonth { get; set; }
    public int BookingsThisMonth { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public List<string> TopRegions { get; set; } = new();
    public Dictionary<string, int> UsersByRole { get; set; } = new();
}

public class FlexiScoreOverrideDto
{
    public int Score { get; set; }
    public string Reason { get; set; } = string.Empty;
}