using FlexiRent.Domain.Enums;

namespace FlexiRent.Application.DTOs;

public class PropertyDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerMonth { get; set; }
    public string Region { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public PropertyType Type { get; set; }
    public PropertyStatus Status { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public double AreaSqft { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}

public class CreatePropertyDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerMonth { get; set; }
    public string Region { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public PropertyType Type { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public double AreaSqft { get; set; }
}

public class UpdatePropertyDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerMonth { get; set; }
    public string Region { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public PropertyType Type { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public double AreaSqft { get; set; }
    public bool IsAvailable { get; set; }
}

public class PropertySearchDto
{
    public string? Region { get; set; }
    public PropertyType? Type { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public string? SearchTerm { get; set; }
    public Guid? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public Guid? NextCursor { get; set; }
    public bool HasMore { get; set; }
    public int TotalCount { get; set; }
}

public class UpdatePropertyStatusDto
{
    public PropertyStatus Status { get; set; }
    public string? Reason { get; set; }
}