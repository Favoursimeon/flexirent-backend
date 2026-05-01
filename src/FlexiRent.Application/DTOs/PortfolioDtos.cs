using FlexiRent.Application.Models;

namespace FlexiRent.Application.DTOs;

public record UploadPortfolioImageDto(FileUpload File, string Title, string? Description, int DisplayOrder = 0);
public record UpdatePortfolioImageDto(string Title, string? Description, int DisplayOrder);
public record PortfolioImageDto(
    Guid Id, Guid OwnerId, string Title, string? Description,
    string ImageUrl, string ContentType, long SizeBytes,
    int DisplayOrder, DateTime CreatedAt, DateTime? UpdatedAt);